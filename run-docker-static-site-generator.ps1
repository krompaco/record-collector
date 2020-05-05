docker build -t record-collector-generator .
New-Item -Path "$pwd" -Name "artifacts\static-site" -ItemType "directory" -Force
docker run -it --mount src="$pwd",target=/sln,type=bind --rm record-collector-generator:latest