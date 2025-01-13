
# This contact's MS's version of dotnet for ubuntu
# Explanation is here: https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#2204-microsoft-package-feed

wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install -y dotnet-sdk-7.0
sudo apt-get install -y aspnetcore-runtime-7.0

## these are to be removed after installation
sudo apt autoremove aspnetcore-targeting-pack-6.0 dotnet-apphost-pack-6.0 dotnet-targeting-pack-6.0 gir1.2-snapd-1 libllvm13 libllvm13:i386
dotnet --list-runtimes
dotnet --list-sdks