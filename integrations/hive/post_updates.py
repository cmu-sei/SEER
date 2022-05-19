#!/usr/bin/python

import argparse
import json
import requests
import time

parser = argparse.ArgumentParser(prog="post_updates", description="Posts hive updates to the SEER API")
parser.add_argument("--input", required=True, help="The input file to use")
args = parser.parse_args()

input_file = args.input

print(f"Reading file: {input_file}")

f = open(input_file, "rb")
json_data = json.load(f)
f.close()

url = "http://host.docker.internal:38080/api/hive/webhook"
headers = {
    'Content-type': 'application/json',
    'Accept': 'application/json'
}

for item in json_data:
    # print(f"{item}")

    try:
        # POST with JSON
        r = requests.post(url, data=json.dumps(item), headers=headers)

        # Response, status etc
        print(f"Response is {r.status_code} {r.text}")
    except:
        print("Error posting to SEER")

    time.sleep(5)

print("Finis")
