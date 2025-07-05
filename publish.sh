dotnet pack -c Release -o ./Deploy/OUT -p:Version=1.9.0
pupnet -k appimage -y
