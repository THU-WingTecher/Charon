LIBIEC_HOME=../..

PROJECT_BINARY_NAME = server_example_basic_io
PROJECT_SOURCES = server_example_basic_io.c
PROJECT_SOURCES += static_model.c

PROJECT_ICD_FILE = simpleIO_direct_control.cid

include $(LIBIEC_HOME)/make/target_system.mk
include $(LIBIEC_HOME)/make/stack_includes.mk

all:	$(PROJECT_BINARY_NAME)

include $(LIBIEC_HOME)/make/common_targets.mk

LDLIBS += -lm

CP = cp
LCOV = lcov
GCOV = gcov
LCOV_TEST_NAME = libiec51850_basic_io

model:	$(PROJECT_ICD_FILE)
	java -jar $(LIBIEC_HOME)/tools/model_generator/genmodel.jar $(PROJECT_ICD_FILE)

$(PROJECT_BINARY_NAME):	$(PROJECT_SOURCES) $(LIB_NAME)
	$(CC) $(CFLAGS) $(LDFLAGS) -o $(PROJECT_BINARY_NAME) $(PROJECT_SOURCES) $(INCLUDES) $(LIB_NAME) $(LDLIBS)
	mkdir -p vmd-filestore
	$(CP) $(PROJECT_BINARY_NAME) vmd-filestore/IEDSERVER.BIN

zero: $(PROJECT_BINARY_NAME)
	$(LCOV) --zerocounters --directory ${LIBIEC_HOME}/build
	$(LCOV) --zerocounters --directory .

lcov: $(PROJECT_BINARY_NAME)
	$(CP) -R ${LIBIEC_HOME}/build .
	$(LCOV) --gcov-tool $(GCOV) -c -t $(LCOV_TEST_NAME) -d . -b . -o "$(LCOV_TEST_NAME).info"
	genhtml "$(LCOV_TEST_NAME).info" -o output

html: lcov
	cd output; python3 -m http.server 8000

clean:
	rm -f $(PROJECT_BINARY_NAME)
	rm -f vmd-filestore/IEDSERVER.BIN
	rm -f *.gcda *.gcno *.info 
	rm -rf output build


