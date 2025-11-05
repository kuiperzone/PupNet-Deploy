dotnet pack -c Release -o ./Deploy/OUT -p:Version=1.9.1
pupnet -k appimage -y
