# newback
multi tenant auth backend rest server for api's. Make ready database end newback will handle the rest.

## scenarios

1) you want to develop frontend without touching backend
2) you should wait for the backend to be developed, but you need to move.
3) you want to prototype ideas, backend/data structures constantly changing

## What `netback` provides
1)   `netback` provides user authentication (correct username/password gives JWT )
2)   provides authorization ( roles for db tables )
3)   rest end points for saving, updating, deleting, listing, searching db tables



## Features
- [x] Authentication using post json
    - [ ] Custom User table
    - [ ] user checking against
    - [ ] throttling limiting warning
    - [ ] 
- [x] Authorization
  - [ ] Forbidden Tables, tables that `netback` should never touch
  - [ ] Table based roles (ie marketing role can reach only marketing)
  - [ ] Limits on tables ( tenant can have 10 item in X table )
- [x] Connects to dbs
    - [x] Mysql
    - [ ] Postgresql
    - [ ] Oracle
    - [ ] Sqlite
    - [ ] Mssql
- [ ] Multi-Tenancy
- [x] Features
  - [x] list everything in table
  - [x] where query
  - [x] insert data
  - [ ] multiple inserts
  - [x] update data
  - [ ] multiple updates
  - [x] delete 1 data
  - [ ] multiple deletes 

# how to use
Spin up docker with connections params, and start using 
 
## Get/List products in table `products`
  http://{{netback}}/products
  
## Get 2nd product in table `products`
-  http://{{netback}}/products/2
-  http://{{netback}}/products?where=id,eq,2

## Get/List ice creams in table `products`
-  http://{{netback}}/products?where=id,in,2;19;22;32
-  http://{{netback}}/products?where=category,eq,icecream
-  http://{{netback}}/products?where=name,like,icecream


## Insert cake into `products`
> POST {{netback}}/Pages
> content-type: application/json
> { 
>    "name":"cake",
>    "category":"not icecream",
>    "price":15,
>    "unknown fields":"ommitted but included in return" 
> }

## Update cake  `products`
POST {{netback}}/Pages
content-type: application/json
{ 
    "id" : 29
    "name":"frozen cake", 
}

  
  