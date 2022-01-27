proc	fun
_bp-4	=	_bp+8	*	_bp+6
_bp-6	=	_bp-4	+	_bp+4
_bp-2	=	_bp-6

wrs	_S0
wri	_bp-2
wrln
endp	fun

proc	three
_t1	=	5
a	=	_t1

_t2	=	10
b	=	_t2

_t3	=	20
d	=	_t3

push	b
push	d
push	a
call 	fun
endp	three

start	proc	three