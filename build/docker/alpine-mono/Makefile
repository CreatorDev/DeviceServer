ORGANISATION?=creatordev
PREFIX?=${ORGANISATION}

ALPINE_MONO_VERSION?=4.2.2.30-r1

VERSION?=latest

.PHONY: all build tag_latest release

all: build

build:
	docker build --build-arg ALPINE_MONO_VERSION=${ALPINE_MONO_VERSION} -t ${PREFIX}/alpine-mono:${ALPINE_MONO_VERSION} --rm .

clean:
	docker rmi ${PREFIX}/alpine-mono || echo "Not found"

tag_latest:
	@if [ "${VERSION}" = "latest" ]; then echo '\nPlease set VERSION\n' && false; fi
	docker tag ${PREFIX}/alpine-mono:${ALPINE_MONO_VERSION}     ${PREFIX}/alpine-mono:latest

release: tag_latest
	docker push ${PREFIX}/alpine-mono:${ALPINE_MONO_VERSION}     && docker push ${PREFIX}/alpine-mono:latest
