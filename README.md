# dndapi

Docker build steps
```
docker build -t dndapi .
docker run -d -p <whatever port you like>:80 --name <somename> dndapi
```

Alternatively you can just run the dndHitpointApi project; the development port's set to 5000.

There's a suite of simple integration tests I wrote for Postman: see DNDBeyondTest.postman_collection.json - you should be able to import it and run it.

The routes for character "management" are set up like this:
```
POST:   /characters/new
GET:    /characters/{id}
DELETE: /characters/delete/{id}
```

For healing/damage/temporary HP, they're like this:
```
(All POST)

/characters/{id}/heal
/characters/{id}/addtemphp
/characters/{id}/damage
```
