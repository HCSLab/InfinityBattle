import os, sys, json, requests, random, time, datetime
from threading import Thread, Lock

from Crypto.PublicKey import RSA
from Crypto.Hash import SHA256
from Crypto.Signature import pkcs1_15

sys.setrecursionlimit(500000)

from flask import Flask, request
app = Flask(__name__)

# import logging
# log = logging.getLogger('werkzeug')
# log.setLevel(logging.ERROR)

Name = None
PubKey = None
PriKey = None
BlockchainAddress = None
Mainchain = None    # Hash ID
fullchain = {}      # All blocks
TempGameList = {}
Report = None

def getMainchain():
    cur = Mainchain
    chain = []
    while cur != None:
        chain.append(fullchain[cur])
        cur = fullchain[cur]['PrevHash']
    chain.reverse()
    return chain

def run(name, pubkey, prikey, port):
    global Name, PubKey, PriKey, BlockchainAddress
    Name, PubKey, PriKey = name, pubkey, prikey
    BlockchainAddress = '{0}:{1}'.format('127.0.0.1', port)
    Load()  # initiate the settings
    # Thread(target=update).start()
    app.run(host='0.0.0.0', port=port)

@app.route('/')
def website():
    head  = f'<title>{Name}\'s Blockchain Home</title>'
    head += f'<link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/css/bootstrap.min.css">'
    head += f'<script src="https://code.jquery.com/jquery-3.5.1.slim.min.js"></script>'
    head += f'<script src="https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js"></script>'
    head += f'<script src="https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.min.js"></script>'
    body  = f'<h2 style="text-align:center; margin-top:10px; ">Welcome, {Name}!</h2>'
    body += f'<div style="word-break:break-all;"><b>Your public key: </b></br> {PubKey}</div>'.replace('\n', '', -1).replace('-----BEGIN PUBLIC KEY-----', '', -1).replace('-----END PUBLIC KEY-----', '', -1)
    confirmed, unconfirmed = revenue(PubKey)
    body += f'<div style="margin-top:10px;"><b>Confirmed Wealth: </b>{confirmed}&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Unconfirmed Wealth: </b>{unconfirmed}</div>'
    body += f'<div style="margin-top:10px"><b>Mainchain Table</b></div>'
    body += f'<table class="table">'
    body += f'<thead><tr><th scope="col">#</th> <th scope="col">Hash</th> <th scope="col">Writer</th> <th scope="col">Bonus</th> <th scope="col">Number of Matches</th> <th scope="col">Full JSON Info</th></tr></thead><tbody>'
    for block in reversed(getMainchain()):
        if block["Height"] == 0:
            body += f'<tr><th scope="row">0</th> <td>{block["Hash"]}</td> <td>Null</td> <td>0</td> <td>NA</td> <td><button type="button" class="btn btn-primary" data-toggle="modal" data-target="#bd-modal-{block["Height"]}">Click Here</button></td></tr>'
        else:
            body += f'<tr><th scope="row">{block["Height"]}</th> <td>{block["Hash"]}</td> <td>...{block["Writer"].replace("-----BEGIN PUBLIC KEY-----", "", -1)[45:64]}...</td> <td>{block["Bonus"]}</td> <td>{len(block["GameData"])}</td><td><button type="button" class="btn btn-primary" data-toggle="modal" data-target="#bd-modal-{block["Height"]}">Click Here</button></td></tr>'
        body += f'<div class="modal fade" id="bd-modal-{block["Height"]}" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">'
        body += '<div class="modal-dialog modal-lg" role="document">'
        body += '<div class="modal-content">'
        body += '<div class="modal-header">'
        body += f'<h5 class="modal-title" id="exampleModalLabel">Block {block["Height"]}</h5>'
        body += '<button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>'
        body += '</div>'
        body += '<div class="modal-body">'
        body += f'<pre>{json.dumps(block, indent=4)}</pre>'
        body += '</div>'
        body += '</div>'
        body += '</div>'
        body += '</div>'
    body += f'</tbody></table>'

    body += f'<div style="margin-top:20px"><b>Pending Match List</b></div>'
    body += f'<table class="table">'
    body += f'<thead><tr><th scope="col">ID</th> <th scope="col">MVP</th> <th scope="col">Full JSON Info</th></tr></thead><tbody>'
    for mid, result in TempGameList.items():
        body += f'<tr><th scope="row">{mid}</th> <td>...{result["MVP"].replace("-----BEGIN PUBLIC KEY-----", "", -1)[45:64]}...</td> <td><button type="button" class="btn btn-primary" data-toggle="modal" data-target="#bd-modal-{mid}">Click Here</button></td></tr>'
        body += f'<div class="modal fade" id="bd-modal-{mid}" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">'
        body += '<div class="modal-dialog modal-lg" role="document">'
        body += '<div class="modal-content">'
        body += '<div class="modal-header">'
        body += f'<h5 class="modal-title" id="exampleModalLabel">Match #{mid}</h5>'
        body += '<button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>'
        body += '</div>'
        body += '<div class="modal-body">'
        body += f'<pre>{json.dumps(result, indent=4)}</pre>'
        body += '</div>'
        body += '</div>'
        body += '</div>'
        body += '</div>'
    body += f'</tbody></table>'

    html = f'<html><head>{head}</head><body style="margin-left:10px; margin-right:10px;">{body}</body></html>'
    return html, 200

@app.route('/blockchain', methods=['GET', 'POST'])
def blockchain():
    global Mainchain, TempGameList, Report
    if request.method == 'GET':
        # get current mainchain
        hashid = request.args.get("hash")
        if hashid == None:
            return json.dumps(getMainchain()), 200
        return json.dumps(fullchain[hashid]), 200
    if request.method == 'POST':
        # update blockchain request 
        ip = request.environ['REMOTE_ADDR']
        port = request.args.get("port")
        print("from", ip, port)
        try:
            block = json.loads(request.get_data())
        except:
            return
        if block['Height'] > fullchain[Mainchain]['Height'] and VerifyBlock(block, ip, port):
            fullchain[block['Hash']] = block
            UpdateGameList(block['Hash'])
            if block['Report'] is not None and Report is not None and block['Report']['Signature'] == Report['Signature']:
                Report = None
            Mainchain = block['Hash']
            Backup()    # backup this new chain to avoid the sudden down
            Thread(target=BroadcastBlock, args=(block, ), daemon=True).start()
            return "Accepted", 200
        if Report is None:
            Report = FindMalicious(block)
        return "Rejected", 200

def VerifyBlock(block, ip, port):
    if block['PrevHash'] is not None and block['PrevHash'] not in fullchain:
        cur = block['PrevHash']
        time.sleep(0.1)
        PrevBlock = json.loads(requests.get(f"http://{ip}:{port}/blockchain?hash={cur}").text)
        if VerifyBlock(PrevBlock, ip, port) is False:
            return False
        fullchain[cur] = PrevBlock
    return True

def FindMalicious(block):
    report_info = {'Hash': block['Hash'], 'Token': block['Token'], 'Bonus': block['Bonus'], 'Signature': block['Signature']}
    cur = Mainchain
    while(fullchain[cur]['Height'] != 0):
        if fullchain[cur]['Token'] == block['Token'] and fullchain[cur]['Writer'] == block['Writer'] and fullchain[cur]['Hash'] != block['Hash']:
            return report_info
        cur = fullchain[cur]['PrevHash']
    return None

def VerifyBlockchain(chain):
    return True

@app.route('/match', methods=['POST'])
def NewMatchRecord():
    match = json.loads(request.get_data())
    mid = match['MatchID']
    # print(mid, 'received')
    if MatchExists(mid, getMainchain()):
        return 'Existed', 200
    # print(mid, "Not exist...")
    if mid in TempGameList or str(mid) in TempGameList:
        return 'Existed', 200
    print(mid, 'Not exist')
    if VerifyMatch(match) is False:
        return 'Rejected', 200
    TempGameList[mid] = match
    # print("====================")
    # print(TempGameList)
    # print("====================")
    if match['MVP'] == PubKey and match['Revenue'][PubKey] >= 500:
        Thread(target=CreateNewBlock, args=(mid, )).start()
    Thread(target=BroadcastMatch, args=(match, )).start()
    return 'Accepted', 200

def MatchExists(mid, chain):
    for block in chain:
        if mid in block["GameData"] or str(mid) in block["GameData"]:
            return True
    return False

def VerifyMatch(match):
    return True

def UpdateGameList(hashid):
    cur = hashid
    while cur != None:
        for identity in fullchain[cur]['GameData']:
            # print(identity, type(identity))
            TempGameList.pop(identity, None)
            TempGameList.pop(eval(identity), None)
        cur = fullchain[cur]['PrevHash']
        if cur is None or cur in fullchain:
            # print("====================")
            # print(TempGameList)
            # print("====================")
            return

@app.route('/pubkey')
def GetPubKey():
    return str(PubKey.replace('-----BEGIN PUBLIC KEY-----','').replace('-----END PUBLIC KEY-----', '')), 200

@app.route('/newIP')
def JotDownIP():
    ip = request.args.get("ip")
    port = request.args.get("port")
    AppendIPList(ip, port)
    return "Accept", 200

# Manipulate on mainchain
def Backup():
    f = open(f'./PoPSystem/config/Blockchain/{Name}.blockchain', 'w')
    f.write(json.dumps(getMainchain()))
    f.close()

def Load():
    global fullchain, Mainchain
    f = open(f'./PoPSystem/config/Blockchain/{Name}.blockchain', 'r')
    chain = json.loads(f.read())
    ips = GetIPList()
    random.shuffle(ips)
    for addr in ips[:min(5, len(ips))]:
        try:
            temp = json.loads(requests.get(f"http://{addr}/blockchain").text)
            if len(temp) > len(chain) and VerifyBlockchain(temp):
                chain = temp
        except:
            time.sleep(0.03)
    for block in chain:
        fullchain[block['Hash']] = block
    Mainchain = chain[-1]['Hash']
    Backup()
# End of mainchain

# Manipulate on ip candidate
def GetIPList():
    f = open(f'./PoPSystem/config/ip.txt', 'r')
    addrs = []
    for line in f.readlines():
        addrs.append(line[:-1])
    f.close()
    random.shuffle(addrs)
    return addrs

def AppendIPList(ip, port):
    addr = '{0}:{1}'.format(ip, port)
    f = open(f'./PoPSystem/config/ip.txt', 'r')
    for line in f.readlines():
        if line[:-1] == addr:
            f.close()
            return
    f.close()
    f = open('./PoPSystem/config/ip.txt', 'a')
    f.write(addr+'\n')
    f.close()
# End of ip candidate

def BroadcastBlock(block):   # This is a helping function for broadcast block
    addrs = GetIPList()
    for addr in addrs:
        print(addr)
        time.sleep(random.random() * 3)
        try:
            port = eval(BlockchainAddress.split(':')[1])
            requests.post(f'http://{addr}/blockchain?port={port}', json.dumps(block))
        except:
            continue
        
def BroadcastMatch(match):   # This is a helping function for broadcast match
    addrs = GetIPList()
    for addr in addrs:
        time.sleep(random.random() * 3)
        try:
            requests.post(f'http://{addr}/match', json.dumps(match))
        except:
            continue

initTarget = (1 << 256) / (1 << 7)
every = 144
eptime = 600

def CreateNewBlock(PendingBlockID):
    global Mainchain
    next_difficulty = -1
    while PendingBlockID in TempGameList:
        timestamp = datetime.datetime.timestamp(datetime.datetime.utcnow())
        block = fullchain[Mainchain]
        if block['Timestamp'] >= timestamp:
            time.sleep(block[-1]['Timestamp'] - timestamp)
            continue
        CurrentGameList = TempGameList.copy()
        report = None
        if Report is not None:
            report = Report.copy()
        preHash = block["Hash"]
        gamedata = ''
        for key in CurrentGameList.keys():
            gamedata += str(key)
        Bonus = len(CurrentGameList) * 150 
        s = str(timestamp) + str(preHash) + str(PubKey) + str(gamedata) + str(PendingBlockID)
        if Report is not None:
            Bonus = Bonus + Report['Bonus']
            s = s + str(Bonus) + str(report['Signature'])
        else:
            s = s + str(Bonus)
        Hash = SHA256.new(s.encode('utf-8')).hexdigest()
        a = int(Hash, 16)
        difficulty = block['Difficulty']
        if (block['Height'] % every == 0 and block['Height'] > 1):
            if next_difficulty == -1:
                chain = getMainchain()
                time_differ = chain[-1]['Timestamp'] - chain[-1-every]['Timestamp']
                difficulty = max(1.0, difficulty * every * eptime / time_differ)
            else:
                difficulty = next_difficulty
        Tvalue = initTarget / difficulty
        promise = Hash + str(PendingBlockID) + str(Bonus)
        signature = pkcs1_15.new(RSA.importKey(PriKey)).sign(SHA256.new(promise.encode('utf-8'))).hex()
        if a < Tvalue:
            block = {
                'Height': block['Height'] + 1,
                'Hash': Hash, 
                'Token': PendingBlockID, 
                'Bonus': Bonus,   # 150 means the processing bonus for each match
                'Timestamp': timestamp, 
                'PrevHash': preHash,
                'Writer': PubKey,
                'Signature': signature,
                'Difficulty': difficulty,
                'Report': report,
                'GameData': CurrentGameList
            }
            if PendingBlockID not in TempGameList:
                break
            requests.post(f"http://{BlockchainAddress}/blockchain?port=0", json.dumps(block))
            break

def factcheck(pubkey, Mainchain, CurrentGameLsit):
    dlist, writers = {}, []
    for match in CurrentGameLsit.values():
        name = match['MVP']
        if name not in dlist:
            dlist[name] = 1
        else:
            dlist[name] += 1
    cur = Mainchain
    m, i = 0, 0
    while(cur != None):
        # print(fullchain)
        writers.append(fullchain[cur]['Writer'])
        if i < every and fullchain[cur]['Writer'] == pubkey:
            m += 1
        for match in fullchain[cur]['GameData'].values():
            name = match['MVP']
            if name not in writers:
                if name not in dlist:
                    dlist[name] = 1
                else:
                    dlist[name] += 1
        cur = fullchain[cur]['PrevHash']
        i = i + 1
    if pubkey not in dlist:
        return max(dlist.values()), -1, m+1
    return max(dlist.values()), dlist[pubkey], m+1

def revenue(pubkey):
    chain = getMainchain()
    confirmed = 0
    unconfirmed = 0
    for block in chain:
        if block['Writer'] == pubkey:
            if block['Height'] > len(chain) - 100:  # 100 means the number of blocks that block reward becomes mature
                unconfirmed = unconfirmed + block['Bonus']   # 150 means the processing bonus for each match
            else:
                confirmed = confirmed + block['Bonus']    # 150 means the processing bonus for each match
        for match in block['GameData'].values():
            if pubkey in match['Revenue']:
                if block['Height'] > len(chain) - 6: # 6 means the number of blocks that a match result is confirmed
                    unconfirmed = unconfirmed + match['Revenue'][pubkey]
                else:
                    confirmed = confirmed + match['Revenue'][pubkey]
    return confirmed, unconfirmed

@app.route('/revenue')
def GetRevenue():
    pubkey = request.args.get('pubkey')
    if pubkey is None:
        pubkey = PubKey
    confirmed, unconfirmed = revenue(pubkey)
    revenue_dict = {'Confirmed': confirmed, 'Unconfirmed': unconfirmed}
    return json.dumps(revenue_dict), 200

# The following part for matchmaking
matchmaking_server = None
teammate, opponent = None, None

@app.route('/startgame')
def StartGame():
    if request.environ['REMOTE_ADDR'] != '127.0.0.1':
        return 'Reject', 404
    Thread(target=CreateMatchRequest, args=(False, ), daemon=True).start()
    return '', 200

@app.route('/startgames')
def StartGameWithTeamforming():
    if request.environ['REMOTE_ADDR'] != '127.0.0.1':
        return 'Reject', 404
    Thread(target=CreateMatchRequest, args=(True, ), daemon=True).start()
    return '', 200

def CreateMatchRequest(finishedTeamForming):
    f = open(f'./PoPSystem/config/bootstrap_ip.txt', 'r')
    addrs = []
    for line in f.readlines():
        addrs.append(line[:-1])
    f.close()
    random.shuffle(addrs)
    for i in range(len(addrs)):
        matchmaking_server = addrs[i]
        if requests.get(f'http://{matchmaking_server}/requestmatch?addr={BlockchainAddress}&teamforming={finishedTeamForming}').status_code == 200:
            break

@app.route('/teammate', methods=['POST', 'GET'])
def setgetteammate():
    global teammate
    if request.method == 'POST':
        teammate = request.get_data()
        return 'Acknowledge', 200
    if request.method == 'GET':
        if teammate is None:
            return '', 200
        else:
            temp = teammate
            teammate = None
            return temp, 200

@app.route('/opponent', methods=['POST', 'GET'])
def setgetopponent():
    global opponent
    if request.method == 'POST':
        opponent = request.get_data()
        return 'Acknowledge', 200
    if request.method == 'GET':
        if opponent is None:
            return '', 200
        else:
            temp = opponent
            opponent = None
            return temp, 200

# Following is for bootstrap server 
pendinglist, singlelist = [], []

@app.route('/requestmatch')
def RequestMatch():
    addr = request.args.get('addr')
    finishedTeamForming = (request.args.get('teamforming') == 'True')
    if finishedTeamForming:
        pendinglist.append([addr, addr])
    else:
        if len(singlelist) == 0:
            singlelist.append(addr)
        else:
            pendinglist.append([singlelist.pop(), addr])
    print(len(pendinglist))
    if len(pendinglist) >= 2:
        print("Now entering clearing round")
        Thread(target=ClearingRound, daemon=True).start()
    return '', 200

def ClearingRound():
    global pendinglist
    while (len(pendinglist) >= 2):
        group = pendinglist[0:2]
        pendinglist = pendinglist[2:]
        print(group)
        for i in [0, 1]:
            if (len(set(group[i])) == 2):
                requests.post(f'http://{group[i][0]}/teammate', group[i][1])
        requests.post(f'http://{group[0][0]}/opponent', group[1][0])
