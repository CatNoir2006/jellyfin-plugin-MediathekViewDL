FROM mcr.microsoft.com/dotnet/sdk:9.0

# Install Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs python3 python3-pip git zip unzip \
    && rm -rf /var/lib/apt/lists/*

# Install JPRM
RUN pip3 install --break-system-packages git+https://github.com/oddstr13/jellyfin-plugin-repository-manager

WORKDIR /src
