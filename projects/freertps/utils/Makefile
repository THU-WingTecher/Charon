BIN=bin
NAME=console
OUT=$(BIN)/$(NAME)
CC=cc

LDFLAGS=-g
CFLAGS=-O2 -g

SRCS=main.cpp lightweightserial.cpp
OBJS=$(SRCS:%.cpp=$(BIN)/%.o)

default: $(BIN) $(OUT)

$(BIN):
	mkdir -p $(BIN)

$(BIN)/%.o: %.cpp
	$(CC) $(CFLAGS) -c $< -o $@

$(OUT): $(OBJS)
	$(CC) $(OBJS) $(LDFLAGS) -o $(OUT)

run: $(OUT)
	@echo "================================================="
	@$(OUT)
	@echo "================================================="
