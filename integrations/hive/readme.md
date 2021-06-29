# TheHive (RE: its use in parallel with SEER)

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
    includedTheHiveOrganisations: []
    excludedTheHiveOrganisations: []
  }
]
```

Start Hive:

```shell
docker-compose up -d
```

Now copy in configuration:

```shell
docker cp application.conf thehive_thehive_1:/etc/thehive/application.conf
```

Might need this to trigger hook as well:

```shell
curl -XPUT -u admin@thehive.local:secret -H 'Content-type: application/json' http://host.docker.internal:9000/api/config/organisation/notification -d '
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

More information:
https://github.com/TheHive-Project/TheHiveDocs/blob/master/admin/webhooks.md

default install user/pass == admin@thehive.local | secret
