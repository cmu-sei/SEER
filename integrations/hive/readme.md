# TheHive (RE: its use in parallel with SEER)

1. Default password to theHive is admin@thehive.local/secret

Webhooks are configured using the webhook key in the configuration file (/etc/thehive/application.conf by default). A minimal configuration contains an arbitrary name and an URL. The URL corresponds to the HTTP endpoint:

Hive v3:

```json
webhooks {
  myLocalWebHook {
    url = "http://host.docker.internal:38080/webhook"
  }
}
```

Hive v4:

```json
notification.webhook.endpoints = [
  {
    name: local
    url: "http://host.docker.internal:38080/api/hive/webhook"
    version: 0
    wsConfig: {}
    auth: {type: "none"}
    includedTheHiveOrganisations: ["*"]
    excludedTheHiveOrganisations: []
  }
]
```

1. Start Hive:

```shell
docker-compose up -d
```

2. Now copy in configuration:

```shell
docker cp application.conf thehive_thehive_1:/etc/thehive/application.conf
```

3. Activate the hook WITH AN ORG ACCOUNT, NOT THE ADMIN ACCOUNT:

```shell
curl -XPUT -u gold@cert.org:secret -H 'Content-type: application/json' http://host.docker.internal:9000/api/config/organisation/notification -d '
{
  "value": [
    {
      "delegate": false,
      "trigger": { "name": "AnyEvent"},
      "notifier": { "name": "webhook", "endpoint": "local" }
    }
  ]
}'

```

4. Restart hive container

More information:
https://github.com/TheHive-Project/TheHiveDocs/blob/master/admin/webhooks.md

default install user/pass == admin@thehive.local | secret
