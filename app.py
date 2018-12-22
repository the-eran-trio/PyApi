from flask import Flask
import json
import summarizerService

app = Flask(__name__)

@app.route("/")
def hello():
    returnJson = {'people':[{'name': 'Scott', 'website': 'stackabuse.com', 'from': 'Nebraska'}]}
    return json.dumps(summarizerService.test(returnJson), indent = 2)