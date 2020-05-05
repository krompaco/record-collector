docker build -t record-collector-nginx .
docker run -d -p 8080:80 record-collector-nginx