# dndapi

Docker build steps
```
docker build -t dndapi .
docker run -d -p <whatever port you like>:80 --name <somename> dndapi
```

Alternatively you can just run the dndHitpointApi project; the development port's set to 5000.

There's a suite of simple integration tests I wrote for Postman: see DNDBeyondTest.postman_collection.json - you should be able to import it and run it. Its requests are pointing at port 5000, so if you're running this in docker you might need to swap ports.

New character service
```
POST:   /characters/new

Request body: the character JSON

Response:
{
    "error": null or an error like {  errorCode:int, errorMessage:string }, 
    "character": { character object }
}

The character object will be manipulated a bit:
- an ID will be assigned
- a hitpoint property will be calculated and added
```

Get character
```
GET:    /characters/{id}

Response:
The character JSON or a 404
```

Delete character
```
DELETE: /characters/delete/{id}

Response:
The character JSON or a 404
```

Healing/TempHP
```
POST: /characters/{id}/heal
POST: /characters/{id}/addtemphp

The request bodies for these are the same JSON:
{
    "value": amountToHealOrAddTempHp
}

Example:
{
    "value":10
}

Response:
{
    "error": null or an error like {  errorCode:int, errorMessage:string },
    "character": { updated char object } 
}
```

Damage
```
/characters/{id}/damage

The request body JSON here takes an array of damage info:
{
    "dealtDamage": [
        {
            "value": num,
            "type": "type"
        },
        {
            "value": num2,
            "type": "type2"
        },
        ...
    ]
}

For example:
{
    "dealtDamage": [
        {
            "value": 3,
            "type": "force"
        },
        {
            "value": 3,
            "type": "fire"
        },
        {
            "value": 3,
            "type": "slashing"
        }
    ]
}

Response:
{
    "error": null or an error like {  errorCode:int, errorMessage:string },
    "character": { updated char object } 
}
```
