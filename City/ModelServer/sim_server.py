import http.server
import socketserver, urllib.parse
import json
from SIRD_model import MakeSimulation

# web request examlpe:
#  http://localhost:8000/sim?city=Msk&disease=smallpox&infinit=5&length=20
#  http://localhost:8000/sim?city=Spb&disease=flu&infinit=25&length=20
# tested with Anaconda 3.4.3
PORT = 8000

# read input
with open('city_data.txt', 'r') as txtfile:
    city_data = json.load(txtfile)
    print("City data:")
    print(city_data)

with open('infection_data.txt', 'r') as txtfile:
    inf_data = json.load(txtfile)
    print("Infection data:")
    print(inf_data)

# получить популяцию города для заражения по названию города
def getCityPop(city_name):
    matches = list((x for x in city_data if x['city'] == city_name))
    if(len(matches) == 0):
        return None
    return float(matches[0]['population'])

# получить параметры инфекции по названию инфекции
def getDisParams(disname):
    matches = list((x for x in inf_data if x['disname'] == disname))
    if(len(matches) == 0):
        return None
    inf_rate = float( matches[0]['inf_rate'] )
    recov_rate = float( matches[0]['recov_rate'] )
    death_rate = float(matches[0]['death_rate'])
    dis_scale = float(matches[0]['dis_scale'])
    return ([inf_rate,recov_rate,death_rate],dis_scale)

#scale = getCityPop('Spb')
#print("scale=",scale)

#inf_params = getDisParams('flu')
#print("inf_params=",inf_params)

def default(instance):
    return {k: v for k, v in vars(instance).items() if not str(k).startswith('_')}

class SIM_DATA:
    def __init__(self):
        self.infected = []
        self.dead = []
        self.recovered = []

class myHandler(http.server.SimpleHTTPRequestHandler):

    def do_GET(self):
        try:
            bits = urllib.parse.urlparse(self.path)
            qs = urllib.parse.parse_qs(bits.query)
            print(bits)
            self.protocol_version='HTTP/1.1'
            self.send_response(200, 'OK')
            self.send_header('Content-type', 'text/html')
            self.end_headers()
            print(qs)
            city = qs['city'][0]
            disease = qs['disease'][0]
            infinit = int(qs['infinit'][0])
            length = int(qs['length'][0])
            #print(city,disease,length,infinit)
            population = getCityPop(city)
            inf_params,dis_scale = getDisParams(disease)
            #print("scale=", population)
            #print("inf_params=", inf_params)
            sd = SIM_DATA
            scale = population * dis_scale
            K = [float(infinit) * 1.0 / scale] + inf_params
            # simulation process
            INF = MakeSimulation(K, length, scale)
            sd.infected = [int(x) for x in INF[0]]
            sd.recovered = [int(x) for x in INF[1]]
            sd.dead = [int(x) for x in INF[2]]
            msg = json.dumps(sd , default=default)
            self.wfile.write(bytes(msg, 'UTF-8'))
        except:
            msg = "Oops! Wrong request"
            self.wfile.write(bytes(msg, 'UTF-8'))

httpd = socketserver.TCPServer(("", PORT), myHandler)
print("Simulation server is working at port", PORT)
httpd.serve_forever()