#! /usr/bin/env python
# https://answers.unity.com/questions/1355795/can-unity-56s-test-runner-optionally-output-nunit.html

import argparse
import os
import sys
import xml.dom.minidom


def convert(input_path, output_path):
    ''' Converts the input file from the NUnit 3 format to NUnit 2 format
        and writes the result into output_path.
    '''
    
    def read_input(input_path):
        dom_tree = xml.dom.minidom.parse(input_path)
        collection = dom_tree.documentElement

        tests = collection.getElementsByTagName('test-case')

        results = {}
        results['name'] = collection.getAttribute('name')
        results['time'] = collection.getAttribute('duration')

        test_results = []
        results['test_results'] = test_results

        num_tests = 0
        num_passed_tests = 0
        num_failed_tests = 0
        num_ignored_tests = 0
        num_skipped_tests = 0

        for test in tests:
            num_tests = num_tests + 1
            curr_test_results = {}
            curr_test_results['name'] = test.getAttribute('fullname')
            test_new_result = test.getAttribute('result')
            test_new_runstate = test.getAttribute('runstate')

            if (test_new_result == 'Passed'):
                # Test Passed...
                num_passed_tests = num_passed_tests + 1
                curr_test_results['state'] = 'Success'
                curr_test_results['result'] = 'Success'
                curr_test_results['executed'] = 'True'
                curr_test_results['state'] = 'Success'
                curr_test_results['success'] = 'True'
                curr_test_results['time'] = test.getAttribute('duration')
            elif (test_new_result == 'Failed'):
                if (test_new_runstate == 'Runnable'):
                    # Test Failed...
                    num_failed_tests = num_failed_tests + 1
                    curr_test_results['state'] = 'Failed'
                    curr_test_results['result'] = 'Failure'
                    curr_test_results['executed'] = 'True'
                    curr_test_results['success'] = 'False'
                    curr_test_results['time'] = test.getAttribute('duration')

                    failure_elem = test.getElementsByTagName('failure')[0]

                    curr_test_results['failure_message'] = failure_elem.getElementsByTagName('message')[0].firstChild.wholeText
                    curr_test_results['failure_stacktrace'] = failure_elem.getElementsByTagName('stack-trace')[0].firstChild.wholeText
                else:
                    # Test could not be run...
                    num_skipped_tests = num_skipped_tests + 1
                    assert(test_new_runstate == 'NotRunnable')
                    curr_test_results['state'] = 'NotRunnable'
                    curr_test_results['result'] = 'NotRunnable'
                    curr_test_results['executed'] = 'False'
                    curr_test_results['success'] = 'False'
                    curr_test_results['time'] = test.getAttribute('duration')

                    curr_test_results['reason_message'] = test.getElementsByTagName('failure')[0].getElementsByTagName('message')[0].firstChild.wholeText
            elif (test_new_result == 'Skipped'):
                # Test was Ignored...
                num_ignored_tests = num_ignored_tests + 1
                curr_test_results['state'] = 'Ignored'
                curr_test_results['result'] = 'Ignored'
                curr_test_results['executed'] = 'False'
                curr_test_results['reason_message'] = test.getElementsByTagName('reason')[0].getElementsByTagName('message')[0].firstChild.wholeText
            else:
                assert(False) #Unknown test result type?
            test_results.append(curr_test_results)

        results['num_tests'] = num_tests
        results['num_passed_tests'] = num_passed_tests
        results['num_failed_tests'] = num_failed_tests
        results['num_ignored_tests'] = num_ignored_tests
        results['num_skipped_tests'] = num_skipped_tests

        date_time = collection.getAttribute('start-time').split(' ')
        results['date'] = date_time[0]
        results['time'] = date_time[1]

        return results

    def write_output(results, output_path):
        # Write XML File (minidom)

        doc = xml.dom.minidom.Document()

        num_tests = results['num_tests']
        num_skipped_tests = results['num_skipped_tests']
        num_ignored_tests = results['num_ignored_tests']
        num_not_run_tests = num_skipped_tests + num_ignored_tests
        num_failed_tests = results['num_failed_tests']

        suite_executed = (num_tests - num_not_run_tests) > 0
        suite_success = num_skipped_tests + num_failed_tests == 0

        root = doc.createElement('test-results')
        root.setAttribute('name', 'Unity Tests')
        root.setAttribute('total', str(num_tests - num_not_run_tests))
        root.setAttribute('errors', str(0))
        root.setAttribute('failures', str(num_failed_tests))
        root.setAttribute('not-run', str(num_not_run_tests))
        root.setAttribute('inconclusive', str(0))
        root.setAttribute('ignored', str(num_ignored_tests))
        root.setAttribute('skipped', str(num_skipped_tests))
        root.setAttribute('invalid', str(0))
        root.setAttribute('date', str(results['date']))
        root.setAttribute('time', str(results['time']))
        doc.appendChild(root)

        test_suite = doc.createElement('test-suite')
        test_suite.setAttribute('name', results['name'])
        test_suite.setAttribute('type', 'Assembly')
        test_suite.setAttribute('executed', 'True' if suite_executed else 'False')
        test_suite.setAttribute('result', 'Success' if suite_success else 'Failure')
        test_suite.setAttribute('success', 'True' if suite_success else 'False')
        test_suite.setAttribute('time', results['time'])
        root.appendChild(test_suite)

        results_elem = doc.createElement('results')
        test_suite.appendChild(results_elem)
        

        test_results = results['test_results']
        for curr_test_results in test_results:

            test_case = doc.createElement('test-case')
            results_elem.appendChild(test_case)

            test_case.setAttribute('name', curr_test_results['name'])
            test_case.setAttribute('executed', curr_test_results['executed'])
            test_case.setAttribute('result', curr_test_results['result'])

            run_state = curr_test_results['state']
            if (run_state == 'Success'):
                # Success...
                test_case.setAttribute('success', curr_test_results['success'])
                test_case.setAttribute('time', curr_test_results['time'])

            elif (run_state == 'Failed'):
                # Failed...
                test_case.setAttribute('success', curr_test_results['success'])
                test_case.setAttribute('time', curr_test_results['time'])

                failure = doc.createElement('failure')
                test_case.appendChild(failure)

                message = doc.createElement('message')
                message_cdata = doc.createCDATASection(curr_test_results['failure_message'])
                message.appendChild(message_cdata)
                failure.appendChild(message)

                stack_trace = doc.createElement('stack-trace')
                stack_trace_cdata = doc.createCDATASection(curr_test_results['failure_stacktrace'])
                stack_trace.appendChild(stack_trace_cdata)
                failure.appendChild(stack_trace)

            elif (run_state == 'NotRunnable'):
                # Not Runnable...
                test_case.setAttribute('success', curr_test_results['success'])
                test_case.setAttribute('time', curr_test_results['time'])
                
                reason = doc.createElement('reason')
                test_case.appendChild(reason)

                message = doc.createElement('message')
                message_cdata = doc.createCDATASection(curr_test_results['reason_message'])
                message.appendChild(message_cdata)
                reason.appendChild(message)

            elif(run_state == 'Ignored'):

                reason = doc.createElement('reason')
                test_case.appendChild(reason)

                message = doc.createElement('message')
                message_cdata = doc.createCDATASection(curr_test_results['reason_message'])
                message.appendChild(message_cdata)
                reason.appendChild(message)

            else:
                print ("Unknown run state: " + run_state)

        doc.writexml( open(output_path, 'w'),
                      indent="    ",
                      addindent="    ",
                      newl='\n')

        doc.unlink()

    results = read_input(input_path)
    write_output(results, output_path)


def main():
    parser = argparse.ArgumentParser(description='Convert an NUnit 3 file to an NUnit 2 file')
    required_named = parser.add_argument_group('Required named arguments')
    required_named.add_argument('-i', '--input', dest='input', help='Input file name', required=True)
    required_named.add_argument('-o', '--output', dest='output', help='Output file name', required=True)
    args = parser.parse_args()

    input_path = args.input
    output_path = args.output

    if (not os.path.isfile(input_path)):
        print ("Input file does not exist")
        return 1

    print ("Converting " + input_path + " to " + output_path)
    convert(input_path, output_path)
    return 0


if __name__ == "__main__":
    sys.exit(main())
