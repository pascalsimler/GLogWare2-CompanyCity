docker run -d --name Oracle19c \
 -p 1521:1521 -p 5000:5500 \
 -e ORACLE_SID=GUDEL \
 -e ORACLE_PDB=GLOGWARE \
 -e ORACLE_PWD=Oramgr001 \
 -e ORACLE_EDITION=standard \
 -v /home/glogware/docker/oracle19c/oradata:/opt/oracle/oradata \
softw/oracle:19.23.0-se
