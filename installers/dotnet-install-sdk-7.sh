cd ~
wget https://download.visualstudio.microsoft.com/download/pr/57150757-56af-450b-ba30-8532fac51e0f/507247327723f24970f66f51554c18bc/dotnet-sdk-7.0.406-linux-x64.tar.gz
mkdir -p ~/.local/lib/dotnet-7.0.406
cd ~/.local/lib/dotnet-7.0.406
tar xvzf ~/dotnet-sdk-7.0.406-linux-x64.tar.gz
# Adjust above if filename is different
export DOTNET_ROOT="$HOME/.local/lib/dotnet-7.0.406"
export PATH="${DOTNET_ROOT}:${PATH}"

# Check the above with:
echo $PATH
echo $DOTNET_ROOT

dotnet --info