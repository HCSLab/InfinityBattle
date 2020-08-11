import os, sys, json, requests, random, time

from threading import Thread

from Crypto.Hash import SHA256
from Crypto.PublicKey import RSA
from Crypto.Signature import pkcs1_15

sys.setrecursionlimit(500000)

from flask import Flask, request
app = Flask(__name__)

# import logging
# log = logging.getLogger('werkzeug')
# log.setLevel(logging.ERROR)

PubKey = None
PriKey = None
BlockchainAddress = None

@app.route('/pubkey')
def GetPubKey():
    return str(PubKey), 200

def run(port, pubkey, prikey, addr):
    global PubKey, PriKey, BlockchainAddress
    PubKey, PriKey = pubkey, prikey
    BlockchainAddress = addr
    app.run(host='0.0.0.0', port=port)

@app.route("/NewMatch", methods=['POST'])
def newMatch():
    # Here is used to store a new match
    if request.environ['REMOTE_ADDR'] != '127.0.0.1':
        return 'Reject', 404
    detail = json.loads(request.get_data())
    store(detail)
    return 'Accept', 200

# Locking the match results
results = {}
shturns = {}
display = {}

@app.route("/gamelist")
def getList():
    return json.dumps(list(display.keys())), 200

def store(detail):
    print("enter store...")
    mid = detail['ID']
    results[mid] = {'Procedure': detail['Procedure'], 'Result': SortByKey(UpdateKeys(detail['Result']))}
    display[mid] = False
    s = json.dumps(results[mid])
    Hash = SHA256.new(s.encode("utf-8"))
    dhash = Hash.hexdigest()
    doubleHash = SHA256.new(dhash.encode("utf-8"))  # sign on its hash
    signature = pkcs1_15.new(RSA.importKey(PriKey)).sign(doubleHash)
    try:
        pkcs1_15.new(RSA.importKey(PubKey)).verify(doubleHash, signature)
        print("Verify success...")
    except:
        print("Verify failed...")
    shturns[mid] = {PubKey: {'Hash': Hash.hexdigest(), 'Signature': signature.hex()}}
    temp = {PubKey: {'Hash': Hash.hexdigest(), 'Signature': signature.hex()}} # for shared turns round one
    Thread(target=SharedTurns, args=(mid, detail['IP'], temp, )).start()
    # pkcs1_15.new(RSA.importKey(PubKey)).verify(content, signature)

def UpdateKeys(result:dict):
    ans = {}
    for key, item in result.items():
        # print(key)
        if key.startswith('b'):
            key = key[2:]
            key = key[:-1]
        k = '-----BEGIN PUBLIC KEY-----' + key + '-----END PUBLIC KEY-----'
        k = k.replace('\\n', '\n')
        ans[k] = item
    return ans

def SortByKey(dictionary:dict):
    ans = {}
    for key in sorted(dictionary.keys()):
        ans[key] = dictionary[key]
    return ans

@app.route("/<int:mid>/result", methods=['POST', 'GET'])
def getResult(mid):
    if request.method == 'POST':
        if request.environ['REMOTE_ADDR'] != '127.0.0.1':
            print('a', request.environ['REMOTE_ADDR'] )
            return 'Reject', 404
        pass
    if request.method == 'GET':
        if display[mid] is False:
            print('b')
            return json.dumps(results[mid]), 404
        return json.dumps(results[mid]), 200

@app.route("/<int:mid>/sharedturns", methods=['POST', 'GET'])
def sharedTurnsRoundOne(mid):
    if request.method == 'POST':
        if mid not in shturns:
            return "Not existed", 404
        info = json.loads(request.get_data())
        pubkey = list(info.keys())[0]
        if pubkey not in results[mid]['Result']:
            print("You are not in the list", pubkey, results[mid]['Result'].keys())
            return "You are not in the list", 404
        sturns = list(info.values())[0]
        # print("sturns:", sturns)
        try:
            signature = bytes.fromhex(sturns['Signature'])
            dhash = sturns['Hash']
            doubleHash = SHA256.new(dhash.encode("utf-8"))
            pkcs1_15.new(RSA.importKey(pubkey)).verify(doubleHash, signature)
            print("Verify success...")
            shturns[mid][pubkey] = sturns
            shturns[mid] = SortByKey(shturns[mid])
            return "Accept", 200
        except Exception as err:
            print(err)
            print("Invalid Matching")
            return "Invalid Matching", 404
    if request.method == 'GET':
        return json.dumps(shturns[mid]), 200

def SharedTurns(mid, IPs, signature):
    print("Thread starts...")
    PostSignatureRoundOne(mid, IPs, signature)
    times = 0
    while (len(shturns[mid]) != len(results[mid]['Result'])):
        time.sleep(random.random() * 3)
        times += 1
        if(times >= 200): return
    print('Received Enough!!!')
    CheckSignatureBoard(mid, IPs)
    Hash = SHA256.new(json.dumps(shturns[mid]).encode('utf-8'))
    results[mid]['SignSharedTurn'] = pkcs1_15.new(RSA.importKey(PriKey)).sign(Hash).hex()
    display[mid] = True
    print('Exposed my result!!! ')
    match = BuildMatchRecord(mid, IPs)
    # f = open('matchMVP.txt', 'w')
    if match is not None:
        # f.write("I am MVP")
        # f.close()
        requests.post(f"http://{BlockchainAddress}/match", json.dumps(match))
    
def PostSignatureRoundOne(mid, IPs, signature):
    addrs = NewIPList(IPs)
    i = -1
    while(len(addrs) > 0):
        i = (i + 1) % len(addrs)
        try:
            response = requests.post(f'http://{addrs[i]}/{mid}/sharedturns', json.dumps(signature))
            if response.status_code == 200:
                addrs.remove(addrs[i])
                i = i - 1
        finally:
            time.sleep(random.random() * 5)
    print('Post DONE!!!')

def CheckSignatureBoard(mid, IPs):
    addrs = NewIPList(IPs)
    i = -1
    while(len(addrs) > 0):
        i = (i + 1) % len(addrs)
        try:
            response = requests.get(f'http://{addrs[i]}/{mid}/sharedturns')
            if response.status_code == 200 and response.text == json.dumps(shturns[mid]):
                addrs.remove(addrs[i])
                i = i - 1
        finally:
            time.sleep(random.random() * 5)
    print("All the same!!!")

def BuildMatchRecord(mid, IPs):
    i, addrs = -1, NewIPList(IPs)
    match = {'MatchID': mid, 'Procedure': [results[mid]['Procedure']], 'Result': [results[mid]['Result']], 'SharedTurn': shturns[mid], 
        'Agreement': {PubKey: results[mid]['SignSharedTurn']}, 'MVP':None, 'Revenue': {}, "MVPSign": None}
    ResultVoting = [[PubKey]]
    NumPlayers = len(addrs) + 1
    SharedTurnsHash = SHA256.new(json.dumps(shturns[mid]).encode('utf-8'))
    while(len(addrs) > 0):
        i = (i + 1) % len(addrs)
        try:
            time.sleep(random.random() * 2)
            pubkey = requests.get(f'http://{addrs[i]}/pubkey').text
            time.sleep(random.random() * 5)
            response = requests.get(f'http://{addrs[i]}/{mid}/result')
            if response.status_code == 200:
                result = json.loads(response.text)
                agreement = bytes.fromhex(result.pop('SignSharedTurn'))
                pkcs1_15.new(RSA.importKey(pubkey)).verify(SharedTurnsHash, agreement)
                match['Agreement'][pubkey] = agreement.hex()
                if shturns[mid][pubkey]['Hash'] != SHA256.new(json.dumps(result).encode('utf-8')).hexdigest():
                    raise ValueError
                match['Procedure'].append(result['Procedure'])
                match['Result'].append(result['Result'])
                ResultVoting.append([pubkey])
                for idx, procedure in enumerate(match['Procedure'][:-1]):
                    if json.dumps(procedure) == json.dumps(result['Procedure']):
                        match['Procedure'].pop()
                        match['Result'].pop()
                        ResultVoting[idx].extend(ResultVoting.pop())
                        break
                print('Agreement from ', addrs[i])
                addrs.remove(addrs[i])
        except ValueError as err:
            print(err)
            print("Invalid Matching...")
        finally:
            time.sleep(random.random() * 5)
    print('Collect all results!!! ')
    print("Match confirmed the mid, procedure, result, sharedturn, agreement")
    for idx, vote in enumerate(ResultVoting):
        if len(vote) > NumPlayers / 2:
            match['Procedure'] = [match['Procedure'][idx]]
            match['Result'] = [match['Result'][idx]]
            RewardList = vote
    # Next we assume there is only one result
    GradeList = {}
    MVPName = None
    TotalGrade = 0
    for pubkey, detail in match['Result'][0].items():
        grade = detail['Killed'] / (detail['Death'] + 1) * (1 + detail['AmountOfHarm'] / 100)
        grade = grade * (1 + detail['AmountOfBear'] / 200) * (1 + detail['AmountOfRescue'] / 150)
        if detail['IsWinner'] is True:
            grade = grade * 1.6
            if MVPName is None or GradeList[MVPName] < grade:
                MVPName = pubkey
        grade = max(2.0, grade)
        TotalGrade += grade
        GradeList[pubkey] = grade
    Revenue = {}
    for pubkey, grade in GradeList.items():
        income = int(500 / NumPlayers + 500 * grade / TotalGrade)
        if pubkey in RewardList:
            Revenue[pubkey] = income
    match['MVP'] = MVPName
    match['Revenue']= Revenue
    if match['MVP'] != PubKey:
        # print(match)
        return None
    match['MVPSign'] = pkcs1_15.new(RSA.import_key(PriKey)).sign(SHA256.new(json.dumps(Revenue).encode('utf-8'))).hex()
    # print(match)
    return match
    
def NewIPList(IPs):
    addrs = []
    for addr in IPs:
        ip, port = addr.split(':')
        requests.get(f"http://{BlockchainAddress}/newIP?ip={ip}&port={eval(port)+1}")
        time.sleep(random.random() * 1)
        port = eval(port) + 2
        addrs.append('{0}:{1}'.format(ip, port))
    return addrs