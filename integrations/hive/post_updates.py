#!/usr/bin/python

import argparse
import json
import requests
import time
import psycopg2

parser = argparse.ArgumentParser(prog="post_updates", description="Posts hive updates to the SEER API")
parser.add_argument("--input", required=False, help="The input file to use")
args = parser.parse_args()

url = "http://host.docker.internal:38080/api/hive/webhook"
headers = {
    'Content-type': 'application/json',
    'Accept': 'application/json'
}

input_file = args.input
if(input_file):
    print(f"Reading file: {input_file}")

    f = open(input_file, "rb")
    json_data = json.load(f)
    f.close()

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
    exit(0)

# use database instead
try:
    connection = psycopg2.connect(user="seer",
                                  password="Scotty@1",
                                  host="host.docker.internal",
                                  port="5432",
                                  database="seer_06132022")
    cursor = connection.cursor()
    postgreSQL_select_Query = "select id, integration_object from assessment_event_history2 where integration_object is not null and integration_object != '' order by id"
    #postgreSQL_select_Query = "select integration_object from assessment_event_history2 where id = 127"

    cursor.execute(postgreSQL_select_Query)
    mobile_records = cursor.fetchall()
    for row in mobile_records:

        id = row[0]
        item = row[1]

        try:
            # POST with JSON
            r = requests.post(url, data=json.dumps(item), headers=headers)

            # Response, status etc
            print(f"{id} response is {r.status_code} {r.text}")
        except:
            print("Error posting to SEER")

        time.sleep(5)

except (Exception, psycopg2.Error) as error:
    print("Error while fetching data from PostgreSQL", error)

finally:
    # closing database connection.
    if connection:
        cursor.close()
        connection.close()
        print("PostgreSQL connection is closed")
