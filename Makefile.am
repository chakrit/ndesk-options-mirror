SUBDIRS = doc

OPTIONS_SRC = \
	$(srcdir)/src/NDesk.Options/AssemblyInfo.cs           \
	$(srcdir)/src/NDesk.Options/NDesk.Options/Options.cs

EXTRA_DIST = $(OPTIONS_SRC) ndesk-options.spec

assemblydir=$(prefix)/lib/ndesk-options
assembly_DATA = src/NDesk.Options/NDesk.Options/Options.cs

net2dir=$(assemblydir)/net2
net2_DATA = $(nolibdir)/net2/NDesk.Options.dll
net35dir=$(assemblydir)/net3.5
net35_DATA=$(nolibdir)/net3.5/NDesk.Options.dll

pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = lib/pkgconfig/ndesk-options-net2.0.pc lib/pkgconfig/ndesk-options-net3.5.pc

nobindir=bin
nolibdir=lib/ndesk-options

CSC = $(MCS)

all: $(nolibdir)/net2/NDesk.Options.dll $(nolibdir)/net3.5/NDesk.Options.dll
	
$(nolibdir)/net2/NDesk.Options.dll: $(OPTIONS_SRC)
	-mkdir -p `dirname $@`
	$(CSC) -debug+ $(OPTIONS_SRC) -t:library -out:$@
	
$(nolibdir)/net3.5/NDesk.Options.dll: $(OPTIONS_SRC)
	-mkdir -p `dirname $@`
	$(CSC) -debug+ -d:LINQ -r:System.Core.dll $(OPTIONS_SRC) -t:library -out:$@

$(nobindir)/options-test2.exe: $(OPTIONS_SRC)
	-mkdir -p `dirname $@`
	$(CSC) -debug+ -d:TEST $(OPTIONS_SRC) -t:exe -out:$@

$(nobindir)/options-test3.exe: $(OPTIONS_SRC)
	-mkdir -p `dirname $@`
	$(CSC) -debug+ -d:LINQ -r:System.Core.dll -d:TEST $(OPTIONS_SRC) -t:exe -out:$@

check: $(nobindir)/options-test2.exe $(nobindir)/options-test3.exe
	mono --debug bin/options-test2.exe && mono --debug bin/options-test3.exe

rpm:
	cp $(DIST_ARCHIVES) /usr/src/packages/SOURCES/ndesk-options-0.1.0.tar.gz && \
		rpmbuild -bb ndesk-options.spec && \
		cp /usr/src/packages/RPMS/noarch/$(distdir)*.rpm .

CLEANFILES = \
	$(nobindir)/options-test2.exe             \
	$(nobindir)/options-test2.exe.mdb         \
	$(nobindir)/options-test3.exe             \
	$(nobindir)/options-test3.exe.mdb         \
	$(nolibdir)/net2/NDesk.Options.dll        \
	$(nolibdir)/net2/NDesk.Options.dll.mdb    \
	$(nolibdir)/net3.5/NDesk.Options.dll	    \
	$(nolibdir)/net3.5/NDesk.Options.dll.mdb

