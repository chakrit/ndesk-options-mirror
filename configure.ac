AC_INIT(src/NDesk.Options/NDesk.Options/Options.cs)
AC_CANONICAL_SYSTEM
AC_PREREQ(2.13)
VERSION=0.1.0
AM_INIT_AUTOMAKE(ndesk-options, $VERSION)
AM_MAINTAINER_MODE
AC_PROG_INSTALL

MONO_REQ_VERSION=1.2.6

AC_PATH_PROG(MCS, gmcs)
AC_PATH_PROG(MONO, mono)
AC_PATH_PROG(MDASSEMBLER, mdassembler)
AC_PATH_PROG(MONODOCER, monodocer)

enable_monodoc=yes

if test "x$MCS" = "x" ; then
  AC_MSG_ERROR([Can't find "gmcs" in your PATH])
fi
if test "x$MONO" = "x" ; then
	AC_MSG_ERROR([Can't find "mono" in your PATH])
fi
if test "x$MDASSEMBLER" = "x" ; then
	enable_monodoc=no
fi
if test "x$MONODOCER" = "x" ; then
	enable_monodoc=no
fi

AC_SUBST(MDASSEMBLER)
AC_SUBST(MONODOCER)

AM_CONDITIONAL(ENABLE_MONODOC, test "x$enable_monodoc" = "xyes")

AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno" ; then
	AC_MSG_ERROR([You need to install pkg-config])
fi
AC_SUBST(PKG_CONFIG)

PKG_CHECK_MODULES(MONO, mono >= $MONO_REQ_VERSION)

AC_OUTPUT([
Makefile
doc/Makefile
lib/pkgconfig/ndesk-options-net2.0.pc
lib/pkgconfig/ndesk-options-net3.5.pc
])
