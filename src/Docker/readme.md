# Docker Database Images for testing

This directory contains a docker compose file that can be used to create database server instances for running tests.

## Before running the tests, from a terminal in this directory, run the following

```bash
$> docker-compose up -d
```

## After running the tests, to cleanup and remove the databases, run the following

```bash
$> docker-compose down
```

## Databases created

|Database|Version|Port|DbName|DbUser|DbPwd|
|--------|-------|----|------|------|-----|
|Firebird|3.0.4|48101|test|test|test|masterkey|
|MySql (mariaDb)|5.5.x|48102|test|test|test|
|Postgres|9.x|48103|test|test|test|