#!/usr/bin/env sh

echo "this text should act like a simple print statement"
echo "testing: setting variables content"
VARNAME="some string value"
echo "testing: echoing variables: $VARNAME"
echo "testing: setting variables to content with variables inside"
HOME="where the heart is"
VARNAME="home is $HOME"
echo "did you know? $VARNAME"
VARNAME="$(echo "I don't really like soup...")"
echo "but did you know? $VARNAME"
logdebug "logging to debug log"
echo "testing" "multiple" "echo" "params"
echo echo without quotes
echo 1 2 3
echo "testing: setting variables to number types"
VARINT=1
VARFLOAT=1.1
echo "testing: enclosed script lines content"
echo "$(echo this should return this string)"
echo "$(echo this is part)" "$(echo of multiple)" "$(echo nested lines)"
echo "testing: accessing array elements"

# TODO: add support for braces in variable expansion
# echo "array key 0: $VARARRAY[0]"
# echo "array key 1: $VARARRAY[1]"
# echo "testing: accessing dictionary elements"
# echo "array key 'key':$VARARRAY['key']"

# async wait function call process mode
# echo "testing: async wait"
# waittest
# echo "this shouldn't be shown until processing is resumed"

# testing calling other scripts as functions
testscript2
echo "is it true: $TESTVAR"
source testscript2
echo "is it true: $TESTVAR"
testscript3 "this is a function param"

# some var setting tests
c="$(a)$(b)"
c="$( (($a + $b)) )"

echo "testing functions"
test_func() {
	echo "this is inside a test function"
	echo "script name: $0"
	echo "param count: $#"
	echo "params: $1 $2 $3"
	echo "params raw: $*"
	LASTCODE=$?
	echo "prev return code: $LASTCODE"
	echo "oh and what about $VARNAME"
}

test_func "testing with" "multiple values inside" 

echo "about to jump to test_func"
test_func
test_func param1 param2 param3

echo "$(test_func will it work?)"
echo "last statement"

echo_func() {
	echo $*
}

echo_func "$(echo_func "multiple nested")" "$(echo_func "lines as")" "$(echo_func "function params")"
echo "last statement"

FUNCRES="$(testscript2)"
echo "$FUNCRES"

# evaluateexpression "1 = 1"
# evaluateexpression ""123" = "123""
# evaluateexpression ""123" = "123""
# evaluateexpression "true"
# evaluateexpression "(2 * 5) + 12 / 6"

if (( 1 = 1 ))
then 
	echo "condition 1 passed"
else
	echo "condition 1 failed"
fi
if (( 1 = 2 ))
then 
	echo "condition 2 passed"
else
	echo "condition 2 failed"
fi

# TESTVAR=0
# while (( $TESTVAR < 10 )) 
# do
# 	echo "inside loop!"
# 	echo "still inside loop!"
# 	TESTVAR="(($TESTVAR + 3))"
# 	echo "loop count: $TESTVAR!"
# 	if (( $TESTVAR < 5 ))
# 	then
# 		echo "low!"		
# 	else
# 		echo "high!"
# 	fi
# done
#
# echo "yay!"

# nestedfunc() {
# 	echo_func "this is inside a function"
# 	echo_func "this is inside the same function"
# 	nestedfunc2
# }
# nestedfunc2() {
# 	echo_func "this is inside a $(echo nested-nested) function"
# }
# nestedfunc
# if [ "$(echo_func 123)" = "123" ]
# then 
# 	echo "condition 1 passed"
# fi
# if (( $VARFLOAT = 1 ))
# then 
# 	echo "condition 1 passed"
# fi
# if (( $VARFLOAT = $VARFLOAT ))
# then 
# 	echo "condition 1 passed"
# fi
# if (( $VARFLOAT = 1 )) || (( 1 <= $VARFLOAT ))
# then 
# 	echo "condition 1 passed"
# fi
# if [ $VARFLOAT = $VARINT ]
# then 
# 	echo "condition 2 passed"
# fi
# if [ "$VARNAME" = "$VARNAME" ] || [ "$VARNAME" = "$VARNAME" ]
# then 
# 	echo_func "$(echo_func condition 3 passed)"
# fi

# # parsing of conditionals
# if [ "$(echo_func "123" )" = "123" ]; 
# then
# 	echo_func "looks like it works"; 
# fi
# if [ "$(echo_func "123" )" = "1233" ] || [ "$(echo_func "123" )" = "1233" ]; 
# then
# 	echo_func "or test";
# fi
# if (( $(echo_func "123" ) == 123 )); 
# then
# 	echo_func "looks like it works";
# fi

#
#
# if statements
# if [ 1 -gt 100]
# then
#   echo omg such a large number
# fi
#
# if [ 1 -gt 100] || [ 1 -le 100]
# then
#   echo uh ok
# fi
#
# if [ "2" == "2" ]
# then
#   echo omg such a large number
# fi
#
# if [ "$SOMEVARVAL" = "1" ]
# then
#   echo "It's equal to 1 yay"
# elif [ "$SOMEVARVAL" = "$(somefunccall random_param_1 another_param)" ]
# then
#   echo did you know? $(echo this is nested!)
# else
#   echo "eh it's actually "$SOMEVARVAL""
# fi

#
# while loops
# counter=1
# while [ $counter -le 10 ]
# do
#   echo count: $counter
#   ((counter++))
# done
#
# for loops
# names="name1 name2 name3"
# for name in $names
# do
#   echo name: $name
# done
#
# for loops range
# for val in {1..5}
# do
#   echo val: $val
# done
#
# multiline with commas
# echo one; echo two; echo three
# echo one; echo "$(echo a; echo b)"; echo three
#
# nested if else else
# if [ "2" = "2" ]
# then
#   if [ "a" = "a" ]
#   then
#     echo omg such a large number
#   else
#     echo not a large number...
#   fi
# else
#   echo "it's an else"
# fi
#
