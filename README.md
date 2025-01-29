# LocalDB
LocalDB is a very simple and inefficient storage system designed to be compatible with [Cactus Messenger](https://github.com/Schlafhase/CactusMessenger).
Because Cactus Messenger usually runs using CosmosDB, LocalDB is designed to be a drop-in replacement for CosmosDB.

## How it works
LocalDB stores every object as a file in a given root directory. 
Each file is named after the object's ID and contains the object as serialized JSON.
