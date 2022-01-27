procedure three is
 a,b,d:integer;
  procedure fun(a:integer;b:integer;d:integer) is
   cc:integer;
  begin

   cc:=a*b+d;
   put("The answer is ");
   putln(cc);
  end fun;
begin
  a:=5;
  b:=10;
  d:=20;
  fun(b,d,a);
end three;