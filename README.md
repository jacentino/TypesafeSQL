TypesafeSQL
===========

Linq-like SQL query builder for use with micro ORMs.

The purpose of TypesafeSQL is to allow for generate SQL queries in typesafe way, thus it's not a real Linq provider to relational database. It provides familiar linq syntax to express queries, but doesn't implement IQueryable. 

Features
--------
* select, where, order by
* inner joins, 
* outer joins (not available with SQL-like syntax)
* group by
* count, sum, avg (only available in queries with group by)
* union, union all, intersect, except
* almost all MS SQL functions
* subqueries (when defined outside a main query)
* method adapting to use with Dapper.NET

Supported databases
-------------------
For now, only MS SQL 2012 is supported. Future releases will support MySQL and PostgreSQL.
