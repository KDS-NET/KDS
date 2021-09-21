# Kinetic Data Structures Simulator for centralized or decentralized scenarios

This repository contains the code of a simulator allowing to simulate a kinetic data structure that is centralized (as usual) or decentralized.

## How to build?

Assuming ubuntu 21.04 (refer to official documentation for other platforms), install the .NET SDK first as such:

```
wget https://packages.microsoft.com/config/ubuntu/21.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-5.0
```

To then build and run a sample project using the library:

```
dotnet run --project ./YourProjectUsing.KDS/YourProjectUsing.KDS.csproj
```
