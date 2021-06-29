# SEER Build and Install Guide

[<-- Main readme](../readme.md)

---

## Build

This builds the container:

```bash
cd src

docker build . -t seer/seer
```

### Export and Import of Images (for air-gapped networks)

Exercise builders often need to build SEER on a high-side network, an easy way to do this is to 
export images as tar files and import them to the target system:

```bash
docker save cmusei/identity > ~/Downloads/identity.tar
docker save seer/seer > ~/Downloads/seer.tar
docker save postgres > ~/Downloads/postgres.tar
```

On the target system, in order to load the tar container files, run:

```bash
cat identity.tar | docker load
cat postgres.tar | docker load
cat seer.tar | docker load
```

### Configuration

Specific configuration for each container:

#### Identity

`seed-data.json` contains a user section we will want to edit as we stand up our environment:

```json

{
    "Username": "ddupdyke@cert.org",
    "Password": "scotty@1",
    "GlobalId": "9fd3c38e-58b0-ffff-80d1-1895af91f1f9"
},

```

Add the users that will be part of your administration team here.

#### Seer

Add the users you would like automatically added to the administrative role under appsettings.json:

```json

"DefaultAdminAccounts": ["ddupdyke", "bsmith", "etc"]

```

Note that identity exports name here, and not a full email address.

### Starting the Containers

To start the stack, run:

```bash
docker-compose up -d
```

With a correct config, on the first request to SEER, it should automatically create and seed the database.