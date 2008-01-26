if ENABLE_MONODOC
SOURCESDIR=$(prefix)/lib/monodoc/sources
TARGETS=ndesk-options-docs.zip ndesk-options-docs.tree ndesk-options-docs.source
else
SOURCESDIR=
TARGETS=
endif

monodocdir=$(SOURCESDIR)
monodoc_DATA=$(TARGETS)

ASSEMBLIES = \
	net2/NDesk.Options.dll \
	net3.5/NDesk.Options.dll

UPDATER = $(MONODOCER) -overrides -pretty -path:en

.PHONY: assemble update update-delete edit clean

assemble: ndesk-options-docs.zip ndesk-options-docs.tree

$(srcdir)/en/examples/NDesk.Options.dll: $(srcdir)/../lib/ndesk-options/net2/NDesk.Options.dll
	cp $^ $@

%.exe: %.cs $(srcdir)/en/examples/NDesk.Options.dll
	gmcs -r:Mono.Posix.dll -lib:en/examples -r:NDesk.Options.dll -out:$@ $<

$(srcdir)/en/examples/locale/es/LC_MESSAGES/localization.mo: $(srcdir)/en/examples/localization-es.po
	msgfmt $< -o $@

$(srcdir)/en/examples/localization.exe: $(srcdir)/en/examples/locale/es/LC_MESSAGES/localization.mo

DOCUMENTATION = \
	$(srcdir)/en/index.xml \
	$(srcdir)/en/ns-NDesk.Options.xml \
	$(srcdir)/en/ns-System.xml \
	$(srcdir)/en/NDesk.Options/Option.xml \
	$(srcdir)/en/NDesk.Options/OptionContext.xml \
	$(srcdir)/en/NDesk.Options/OptionException.xml \
	$(srcdir)/en/NDesk.Options/OptionSet.xml \
	$(srcdir)/en/NDesk.Options/OptionValueType.xml \
	$(srcdir)/en/System/Action`2.xml

EXAMPLES_OUTPUT = \
	en/examples/context.txt \
	en/examples/greet.txt \
	en/examples/localization.txt \
	en/examples/subclass.txt

%.txt: %.in %.exe
	-rm $@
	exec 3<$< ; \
	while read line 0<&3 ; do \
		if test -n "$$line" ; then \
			echo "$$ $$line" | sed 's#en/examples/##' >> $@ ; \
			sh -c "$$line" >> $@ ; \
		else \
			echo "" >> $@ ; \
		fi ; \
	done ; \
	exec 3>&-;

ndesk-options-docs.zip ndesk-options-docs.tree: $(DOCUMENTATION)
	$(MDASSEMBLER) --ecma $(srcdir)/en -o ndesk-options-docs

update: $(addprefix ../lib/ndesk-options/, $(ASSEMBLIES)) $(srcdir)/en/examples/*.cs $(EXAMPLES_OUTPUT)
	for a in $(ASSEMBLIES) ; do \
		$(UPDATER) -assembly:../lib/ndesk-options/$$a ; \
	done

update-delete: $(addprefix ../lib/ndesk-options/, $(ASSEMBLIES)) $(srcdir)/en/examples/*.cs $(EXAMPLES_OUTPUT)
	for a in $(ASSEMBLIES) ; do \
		$(UPDATER) --delete -assembly:../lib/ndesk-options/$$a ; \
	done

EXTRA_DIST = \
	$(srcdir)/ndesk-options-docs.source \
	$(srcdir)/en/examples/context.cs \
	$(srcdir)/en/examples/context.in \
	$(srcdir)/en/examples/context.txt \
	$(srcdir)/en/examples/greet.cs \
	$(srcdir)/en/examples/greet.in \
	$(srcdir)/en/examples/greet.txt \
	$(srcdir)/en/examples/localization.cs \
	$(srcdir)/en/examples/localization.in \
	$(srcdir)/en/examples/localization.txt \
	$(srcdir)/en/examples/locale/es/LC_MESSAGES/localization.mo \
	$(DOCUMENTATION)

CLEANFILES = \
	ndesk-options-docs.zip \
	ndesk-options-docs.tree \
	en/examples/NDesk.Options.dll \
	en/examples/*.exe \
	$(EXAMPLES_OUTPUT)

edit:
	monodoc --edit $(srcdir)/en

clean:
	-rm $(CLEANFILES)
