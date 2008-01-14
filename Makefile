
OPTIONS_SRC = \
	src/NDesk.Options/AssemblyInfo.cs           \
	src/NDesk.Options/NDesk.Options/Options.cs

bindir=bin
libdir=lib/ndesk-options

CSC = gmcs

all: $(libdir)/net2/NDesk.Options.dll \
		$(libdir)/net3.5/NDesk.Options.dll
	
$(libdir)/net2/NDesk.Options.dll: $(OPTIONS_SRC)
	$(CSC) -debug+ $(OPTIONS_SRC) -t:library -out:$@
	
$(libdir)/net3.5/NDesk.Options.dll: $(OPTIONS_SRC)
	$(CSC) -debug+ -d:LINQ -r:System.Core.dll $(OPTIONS_SRC) -t:library -out:$@

$(bindir)/options-test2.exe: $(OPTIONS_SRC)
	$(CSC) -debug+ -d:TEST $(OPTIONS_SRC) -t:exe -out:$@

$(bindir)/options-test3.exe: $(OPTIONS_SRC)
	$(CSC) -debug+ -d:LINQ -r:System.Core.dll -d:TEST $(OPTIONS_SRC) -t:exe -out:$@

check: $(bindir)/options-test2.exe $(bindir)/options-test3.exe
	mono --debug bin/options-test2.exe && mono --debug bin/options-test3.exe

clean:
	-rm \
		$(bindir)/options-test2.exe           \
		$(bindir)/options-test2.exe.mdb       \
		$(bindir)/options-test3.exe           \
		$(bindir)/options-test3.exe.mdb       \
		$(libdir)/net2/NDesk.Options.dll       \
		$(libdir)/net2/NDesk.Options.dll.mdb   \
		$(libdir)/net3.5/NDesk.Options.dll	      \
		$(libdir)/net3.5/NDesk.Options.dll.mdb   \

