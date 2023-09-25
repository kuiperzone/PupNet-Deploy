dotnet pack -c Release -o ./Deploy/OUT -p:Version=1.7.0
pupnet -r linux-x64 -k deb -y
pupnet -r linux-x64 -k rpm -y
pupnet -r linux-x64 -k appimage -y
pupnet -r linux-arm64 -k appimage -y
pupnet -r linux-arm -k appimage -y
