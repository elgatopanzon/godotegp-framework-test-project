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
c="$( ((a + b)) )"

echo "testing functions"
test_func() {
	echo "this is inside a test function"
	echo "script name: $0"
	echo "param count: $#"
	echo "params: $1 $2 $3"
	echo "params raw: $*"
	LASTCODE=$?
	echo "prev return code: $LASTCODE"
}

echo "about to jump to test_func"
test_func
test_func param1 param2 param3


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
#   echo It's equal to 1 yay
# elif [ "$SOMEVARVAL" = "$(somefunccall random_param_1 another_param)" ]
# then
#   echo did you know? $(echo this is nested!)
# else
#   echo eh it's actually "$SOMEVARVAL"
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
#   echo it's an else
# fi
#
