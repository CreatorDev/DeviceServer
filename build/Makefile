ORGANISATION?=creatordev
PREFIX?=${ORGANISATION}
SOURCE_IMAGE?=${ORGANISATION}/dotnet-mono-base

# Define which projects are executables and which are libraries available via nuget
SERVICES='Imagination.LWM2M.Bootstrap Imagination.LWM2M.Server Imagination.Service.ChangeNotification Imagination.Service.Subscription Imagination.Service.Webhook Imagination.WebService.DeviceServer'
PACKAGES='Imagination.Common Imagination.LWM2M.Common Imagination.Model Imagination.Service.Common Imagination.WebService.Common'

BUILD_NUMBER?=1

.PHONY: usage build_source build nuget_pack nuget_push publish docker clean

all: clean build_base nuget_pack service_publish docker

define HEADER
	@echo
	@echo "*********************************************************************"
	@echo "*"
	@echo "*   $(1)"
	@echo "*"
	@echo "*********************************************************************"
	@echo
endef

build_base:
	$(call HEADER,$@)
	docker build \
	-t '$(SOURCE_IMAGE):latest' docker/dotnet-mono-base

build_source:
	$(call HEADER,$@)
	docker run --rm \
	  -v $(dir $(CURDIR)):/app \
	  -w /app \
	  ${SOURCE_IMAGE} \
	  bash build/scripts/build_source.sh

nuget_pack: build_source
	$(call HEADER,$@)
	docker run --rm \
	  -v $(dir $(CURDIR)):/app \
	  -v $(CURDIR)/scripts:/scripts \
	  -v $(CURDIR)/output:/output \
	  -w /app/src \
	  -e PACKAGES=$(PACKAGES) \
	  -e CI_BUILD_NUMBER=$(BUILD_NUMBER) \
	  ${SOURCE_IMAGE} \
	  bash /scripts/nuget_pack.sh

nuget_push:
	$(call HEADER,$@)
	docker run --rm \
	  -v $(dir $(CURDIR)):/app \
	  -v $(CURDIR)/scripts:/scripts \
	  -v $(CURDIR)/output:/output \
	  -w /output/nuget \
	  -e NUGETSERVER=$(NUGETSERVER) \
	  -e NUGETKEY=$(NUGETKEY) \
	  ${SOURCE_IMAGE} \
	  bash /scripts/nuget_push.sh

service_publish: build_source
	$(call HEADER,$@)
	docker run --rm \
	  -v $(dir $(CURDIR)):/app \
	  -v $(CURDIR)/scripts:/scripts \
	  -v $(CURDIR)/output:/output \
	  -w /app/src \
	  -e SERVICES=$(SERVICES) \
	  -e CI_BUILD_NUMBER=$(BUILD_NUMBER) \
	  ${SOURCE_IMAGE} \
	  bash /scripts/service_publish.sh

docker:
	$(call HEADER,$@)
	# be sure to check docker/Makefile input variables
	$(MAKE) -C docker

clean:
	$(call HEADER,$@)
	rm -frv output/

