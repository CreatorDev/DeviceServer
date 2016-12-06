FROM creatordev/alpine-glibc

RUN apk add --no-cache --virtual=.build-dependencies wget ca-certificates tar xz && \
    wget "https://www.archlinux.org/packages/extra/x86_64/mono/download/" -O "/tmp/mono.pkg.tar.xz" && \
    tar -xJf "/tmp/mono.pkg.tar.xz" && \
    cert-sync /etc/ssl/certs/ca-certificates.crt && \
    apk add --no-cache libuv && \
    apk del .build-dependencies && \
    rm /tmp/* && \
    ln -s /usr/lib/libuv.so.1.0.0 /usr/lib/libuv.so