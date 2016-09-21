FROM alpine:3.4

ARG ALPINE_MONO_VERSION
ENV ALPINE_MONO_VERSION ${ALPINE_MONO_VERSION:-4.2.2.30-r1}

RUN echo "@testing http://dl-4.alpinelinux.org/alpine/edge/testing" >> /etc/apk/repositories \
    && apk add --no-cache --update mono@testing=${ALPINE_MONO_VERSION} libuv \
    && ln -s /usr/lib/libuv.so.1.0.0 /usr/lib/libuv.so \
    && rm -rf /var/cache/apk/*