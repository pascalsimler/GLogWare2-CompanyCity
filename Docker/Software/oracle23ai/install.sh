docker run -d --name Oracle23 \
 -p 1521:1521 -p 5000:5500 \
 -e ORACLE_PDB=GLOGWARE \
 -e ORACLE_PWD=Oramgr001 \
 -e ORACLE_EDITION=standard \
 -v /home/glogware/docker/oracle23/oradata:/opt/oracle/oradata \
 oriolrt/oracle-23ai
