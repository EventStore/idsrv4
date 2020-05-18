# Identity Server 4 Docker Container Image

This container image contains [IdentityServer 4](https://github.com/IdentityServer/IdentityServer4/) plus the [in memory quickstart UI](https://github.com/IdentityServer/IdentityServer4.Templates). The intended use is for integration testing your authentication pipeline.

## Getting started

```bash
docker pull eventstore/idsrv4

docker run \
    --rm -it \
    -p 5000:5000 \                                            # HTTP port
    -p 5001:5001 \                                            # HTTPS port
    --volume $PWD/users.conf.json:/etc/idsrv4/users.conf \    # mount users file; required
    --volume $PWD/idsrv4.conf.json:/etc/idsrv4/idsrv4.conf \  # mount configuration file
    eventstore/idsrv4
```

## Example `users.conf`

The `users.conf` file must contain json array matching the schema of a `TestUser` and must be mounted to `/etc/idsrv4/users.conf`:

```json
[{
  "subjectId": "818727",
  "username": "alice",
  "password": "alice",
  "claims": [{
    "type": "name",
    "value": "Alice Smith"  
  }, {
    "type": "email",
    "value": "AliceSmith@email.com"
  }]
}]
```

## Example `idsrv4.conf`

The `idsrv4.conf` file should contain configuration for [`IdentityResources`](https://identityserver4.readthedocs.io/en/latest/reference/identity_resource.html), [`ApiResources`](https://identityserver4.readthedocs.io/en/latest/reference/api_resource.html), and [`Clients`](https://identityserver4.readthedocs.io/en/latest/reference/client.html), and must be mounted to `/etc/idsrv4/idsrv4.conf`. There are many ways to set these up, please refer to the [IdentityServer4 documentation](https://identityserver4.readthedocs.io) or [this example](https://github.com/IdentityServer/IdentityServer4.Templates/blob/553d22b/src/IdentityServer4InMem/appsettings.json) for more information. 
