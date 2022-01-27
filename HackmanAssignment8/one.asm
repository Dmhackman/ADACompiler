	.model small
	.586
	.stack 100h
	.data
_S0	DB	"The answer is ", "$"
_t1	DW	?
_t2	DW	?
_t3	DW	?
a	DW	?
b	DW	?
d	DW	?
	.code
	include io.asm

start	PROC
	mov ax, @data
	mov ds, ax
	call three
	mov ah, 04ch
	int 21h
start	ENDP

three	PROC
	push bp
	mov bp, sp
	sub sp, 6
	mov ax, 5
	mov [_t1], ax
	mov ax, [_t1]
	mov [a], ax
	mov ax, 10
	mov [_t2], ax
	mov ax, [_t2]
	mov [b], ax
	mov ax, 20
	mov [_t3], ax
	mov ax, [_t3]
	mov [d], ax
	push b
	push d
	push a
	call fun
	add sp, 6
	pop bp
	ret 0
three	endp

fun	PROC
	push bp
	mov bp, sp
	sub sp, 6
	mov ax, [bp+8]
	mov bx, [bp+6]
	imul bx
	mov [bp-4], ax
	mov ax, [bp-4]
	add ax, [bp+4]
	mov [bp-6], ax
	mov ax, [bp-6]
	mov [bp-2], ax
	mov dx, OFFSET _S0
	call writestr
	mov ax, [bp-2]
	mov dx, ax
	call writeint
	call writeln
	add sp, 6
	pop bp
	ret 6
fun	endp

main	PROC
	call three
main	endp
END	start
