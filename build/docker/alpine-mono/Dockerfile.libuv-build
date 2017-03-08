FROM creatordev/alpine-glibc
ARG LIBUV_VER=1.11.0
RUN apk add --no-cache --virtual=.build-dependencies wget ca-certificates tar xz automake autoconf m4 gcc g++ libtool make && \
    wget "https://www.archlinux.org/packages/extra/x86_64/mono/download/" -O "/tmp/mono.pkg.tar.xz" && \
    wget https://github.com/libuv/libuv/archive/v$LIBUV_VER.tar.gz -O /tmp/libuv-v$LIBUV_VER.tar.gz && \
    tar xf "/tmp/mono.pkg.tar.xz" && \
    tar xf "/tmp/libuv-v$LIBUV_VER.tar.gz" -C /tmp && \
    cd /tmp/libuv-$LIBUV_VER && \
    ./autogen.sh && \
    ./configure --prefix=/usr && \
    make && make install && \
    cert-sync /etc/ssl/certs/ca-certificates.crt && \
    apk del .build-dependencies && \
    rm -rf /tmp/* /usr/lib/*.a /usr/include/*