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
pkgconfig_DATA = lib/pkgconfig/ndesk-options.pc lib/pkgconfig/ndesk-options-linq.pc

nobindir=bin
nolibdir=lib/ndesk-options

CSC = $(MCS)

.PHONY: all check rpm

all: $(nolibdir)/net2/NDesk.Options.dll $(nolibdir)/net3.5/NDesk.Options.dll

dist-hook:
	if [ -d $(srcdir)/.git ] ; then \
		(cd $(srcdir) && git-log > $(distdir)/ChangeLog) ; \
	fi
	
$(nolibdir)/net2/NDesk.Options.dll: $(OPTIONS_SRC)
	-mkdir -p `dirname $@`
	$(CSC) -debug+ $(OPTIONS_SRC) -t:library -out:$@
	
$(nolibdir)/net3.5/NDesk.Options.dll: $(OPTIONS_SRC)
	-mkdir -p `dirname $@`
	$(CSC) -debug+ -d:LINQ -r:System.Core.dll $(OPTIONS_SRC) -t:library -out:$@

$(nobindir)/options-test.exe: $(OPTIONS_SRC)
	-mkdir -p `dirname $@`
	$(CSC) -debug+ -d:TEST $(OPTIONS_SRC) -t:exe -out:$@

$(nobindir)/options-test-linq.exe: $(OPTIONS_SRC)
	-mkdir -p `dirname $@`
	$(CSC) -debug+ -d:LINQ -r:System.Core.dll -d:TEST $(OPTIONS_SRC) -t:exe -out:$@

check: $(nobindir)/options-test.exe $(nobindir)/options-test-linq.exe
	mono --debug bin/options-test.exe && mono --debug bin/options-test-linq.exe

rpm:
	cp $(DIST_ARCHIVES) /usr/src/packages/SOURCES/ndesk-options-0.1.0.tar.gz && \
		rpmbuild -bb ndesk-options.spec && \
		cp /usr/src/packages/RPMS/noarch/$(distdir)*.rpm doc/web/archive

bin-zip:
	-rm doc/web/archive/$(distdir).bin.zip
	$(MAKE) prefix= DESTDIR=$(distdir).bin install && \
		zip -r doc/web/archive/$(distdir).bin.zip $(distdir).bin && \
		rm -Rf $(distdir).bin

CLEANFILES = \
	$(nobindir)/options-test.exe              \
	$(nobindir)/options-test.exe.mdb          \
	$(nobindir)/options-test-linq.exe         \
	$(nobindir)/options-test-linq.exe.mdb     \
	$(nolibdir)/net2/NDesk.Options.dll        \
	$(nolibdir)/net2/NDesk.Options.dll.mdb    \
	$(nolibdir)/net3.5/NDesk.Options.dll	    \
	$(nolibdir)/net3.5/NDesk.Options.dll.mdb

