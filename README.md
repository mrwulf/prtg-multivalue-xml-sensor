prtg-multivalue-xml-sensor
==========================

Requires net-gnu-getopt from here: https://github.com/mrwulf/net-gnu-getopt

Here's an example command line:

    MultiValueXML.exe -u http://172.16.81.142:8081/mbean?objectname=org.apache.cassandra.db:type=ColumnFamilies,keyspace=commons,columnfamily=items_&template=identity 
    -x /MBean/Attribute 
    -k @name 
    -v @value


Here's the parameters to pass in from PRTG:
* For ThreadPool Statistics from cassandra:
    ```
    -u http://%host:8081/mbean?objectname=org.apache.cassandra.request:type=ReadStage&template=identity 
    -x /MBean/Attribute 
    -k @name 
    -v @value
```

* For ColumnFamily Statistics from cassandra:
    ```
    -u http://%host:8081/mbean?objectname=org.apache.cassandra.db:type=ColumnFamilies,keyspace=commons,columnfamily=items_&template=identity 
    -x /MBean/Attribute 
    -k @name 
    -v @value
```
