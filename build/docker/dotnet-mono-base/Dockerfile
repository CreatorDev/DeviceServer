FROM debian:jessie

# Install mono
RUN apt-get update \
	&& apt-get install -y curl wget \
	&& rm -rf /var/lib/apt/lists/*


# This can hang on some systems ...
#RUN apt-key adv --keyserver pgp.mit.edu --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF

# ... so we do this instead:
ADD key.txt /tmp
RUN cat /tmp/key.txt | apt-key add -

RUN echo "deb http://download.mono-project.com/repo/debian wheezy main" > /etc/apt/sources.list.d/mono-xamarin.list \
        && echo "deb http://download.mono-project.com/repo/debian wheezy-libjpeg62-compat main" | tee -a /etc/apt/sources.list.d/mono-xamarin.list \
        && apt-get update --fix-missing \
	&& apt-get install -y mono-devel ca-certificates-mono fsharp mono-vbnc nuget \
        && rm -rf /var/lib/apt/lists/*

# Install .NET Core dependencies
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libc6 \
        libcurl3 \
        libgcc1 \
        libicu52 \
        liblttng-ust0 \
        libssl1.0.0 \
        libstdc++6 \
        libtinfo5 \
        libunwind8 \
        libuuid1 \
        zlib1g \
    && rm -rf /var/lib/apt/lists/*


# .NET runtime

#RUN apt-get update \
#    && apt-get install -y --no-install-recommends \
#        ca-certificates \
#        curl \
#    && rm -rf /var/lib/apt/lists/*

# Install .NET Core
#ENV DOTNET_CORE_VERSION 1.0.0-rc2-3002702
#RUN curl -SL https://dotnetcli.blob.core.windows.net/dotnet/beta/Binaries/$DOTNET_CORE_VERSION/dotnet-debian-x64.$DOTNET_CORE_VERSION.tar.gz --output dotnet.tar.gz \
#    && mkdir -p /usr/share/dotnet \
#    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
#    && rm dotnet.tar.gz \
#    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

# .NET Development

# Work around https://github.com/dotnet/cli/issues/1582 until Docker releases a
# fix (https://github.com/docker/docker/issues/20818). This workaround allows
# the container to be run with the default seccomp Docker settings by avoiding
# the restart_syscall made by LTTng which causes a failed assertion.
ENV LTTNG_UST_REGISTER_TIMEOUT 0


# Install .NET CLI dependencies
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        clang-3.5 \
        libc6 \
        libcurl3 \
        libgcc1 \
        libicu52 \
        liblttng-ust0 \
        libssl1.0.0 \
        libstdc++6 \
        libtinfo5 \
        libunwind8 \
        libuuid1 \
        zlib1g \
    && rm -rf /var/lib/apt/lists/*

# Install .NET Core SDK
ENV DOTNET_CORE_SDK_VERSION 1.0.0-preview1-002702
RUN curl -SL https://dotnetcli.blob.core.windows.net/dotnet/beta/Binaries/$DOTNET_CORE_SDK_VERSION/dotnet-dev-debian-x64.$DOTNET_CORE_SDK_VERSION.tar.gz --output dotnet.tar.gz \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet


ENV DOTNET_REFERENCE_ASSEMBLIES_PATH=/usr/lib/mono/xbuild-frameworks

