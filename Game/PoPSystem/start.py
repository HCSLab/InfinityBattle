import sys, os, json, datetime, time
import blockchain, game
from hashlib import sha256
from multiprocessing import Process

from Crypto.PublicKey import RSA

def init_key():
    key = RSA.generate(2048)
    return key.publickey().export_key(), key.export_key()

def user_info(name, pubkey, prikey):
    dict = {
        'name': name, 
        'PublicKey': pubkey.decode('utf-8'), 
        'PrivateKey': prikey.decode('utf-8')
    }
    return dict

def genesis():
    timestamp = datetime.datetime.timestamp(datetime.datetime.utcnow())
    Hash = sha256(str(timestamp).encode()).hexdigest()
    block = {
        'Height': 0,
        'Hash': Hash, 
        'Timestamp': timestamp, 
        'PrevHash': None,
        'Writer': None,
        'Difficulty':1.0,
        'GameData':{}
    }
    return block

def Init(user):
    if not os.path.exists(f'./PoPSystem/config'):
        os.makedirs(f'./PoPSystem/config')
        os.makedirs(f'./PoPSystem/config/Blockchain')
    if not os.path.exists(f'./PoPSystem/config/{user}.json'):
        pubKey, priKey = init_key()
        f = open(f'./PoPSystem/config/{user}.json', 'w')
        f.write(json.dumps(user_info(user, pubKey, priKey)))
        f.close()
    if not os.path.exists(f'./PoPSystem/config/Blockchain/{user}.blockchain'):
        chain = [genesis()]
        f = open(f'./PoPSystem/config/Blockchain/{user}.blockchain', 'w')
        f.write(json.dumps(chain))
        f.close()


if __name__ == "__main__":
    user = sys.argv[1]
    port = eval(sys.argv[2])
    Init(user)  # initiate user's settings
    f = open(f'./PoPSystem/config/{user}.json', 'r')
    info = json.loads(f.read())
    f.close()
    # (port+1) for blockchain; (port+2) for P2P_game_port
    Process(target=blockchain.run, args=(user, info['PublicKey'], info['PrivateKey'], port+1, )).start()
    Process(target=game.run, args=(port+2, info['PublicKey'], info['PrivateKey'], '{0}:{1}'.format('127.0.0.1', port+1), )).start()