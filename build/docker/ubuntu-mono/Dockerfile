FROM ubuntu
RUN BUILD_PACKAGES="wget xz-utils automake libtool gcc g++ curl make" && \
    apt-get update && \
    apt-get install -y $BUILD_PACKAGES && \
    AUTO_ADDED_PACKAGES=`apt-mark showauto` && \
    wget "https://www.archlinux.org/packages/extra/x86_64/mono/download/" -O "/tmp/mono.pkg.tar.xz" && \
    tar -xJf "/tmp/mono.pkg.tar.xz" && \
    curl -sSL https://github.com/libuv/libuv/archive/v1.9.1.tar.gz | tar zxf - -C /tmp && \
    cd /tmp/libuv-1.9.1 && \
    ./autogen.sh && \
    ./configure --prefix=/usr && \
    make && make install && \
    apt-get remove --purge -y $BUILD_PACKAGES $AUTO_ADDED_PACKAGES && \
    apt-get install -y tar ca-certificates && \
    rm -rf /tmp/* /usr/lib/*.a /usr/include/*