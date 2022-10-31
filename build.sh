dotnet tool install --global dotnet-sonarscanner

dotnet build-server shutdown

dotnet sonarscanner begin /k:"ben" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="sqp_e8c93696ac5c32046157ee5c2f88d3ee82ca1c45" /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

dotnet build

dotnet test -p:coverletOutput=./coverage.opencover.xml -p:CollectCoverage=true -p:CoverletOutputFormat=opencover
    
dotnet sonarscanner end /d:sonar.login="sqp_e8c93696ac5c32046157ee5c2f88d3ee82ca1c45"
