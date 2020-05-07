docker build -t record-collector-generator .
docker run -it --mount src="$pwd",target=/sln,type=bind --rm record-collector-generator:latest