#include <stdio.h>
#include "freertps/freertps.h"
#include <stdio.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <stdlib.h>
#include <errno.h>
#include <string.h>

/* An array of one message (aka sample in dds terms) will be used. */
#define MAX_SAMPLES 1000


#include <pthread.h>
#include <stdio.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <stdlib.h>
#include <errno.h>
#include <string.h>
#include <stdbool.h>
#include <unistd.h>

#define BUFLEN  1024

#define PORT 7400
#define PORT1 7401
#define IPADDR  "239.255.0.1"


int test_pthread(pthread_t tid) /*pthread_kill的返回值：成功（0） 线程不存在（ESRCH） 信号不合法（EINVAL）*/
{
    int pthread_kill_err;
        pthread_kill_err = pthread_kill(tid,0);

    if(pthread_kill_err == ESRCH)
            return 0;
    else if(pthread_kill_err == EINVAL)
            return 1;
    else
            return 2;
}

void*
thread_worker1(void* agr)
{
    usleep(5000);
    int sfd = socket(AF_INET, SOCK_DGRAM, 0);
	struct sockaddr_in servaddr;
	memset(&servaddr, 0, sizeof(servaddr));
	servaddr.sin_family = AF_INET;
	servaddr.sin_port = htons(PORT);
	servaddr.sin_addr.s_addr = inet_addr(IPADDR);

    int sfd1 = socket(AF_INET, SOCK_DGRAM, 0);
    struct sockaddr_in servaddr1;
	memset(&servaddr1, 0, sizeof(servaddr1));
	servaddr1.sin_family = AF_INET;
	servaddr1.sin_port = htons(PORT1);
	servaddr1.sin_addr.s_addr = inet_addr(IPADDR);

    printf("%s\n",agr);

    FILE *file;
    char buf[BUFLEN];
    file=fopen(agr,"r");
    if(!file)
    {
        puts("can't open file!");
        return NULL;
    }

    while(fgets(buf,BUFLEN,file))//读取TXT中字符
    {
        sendto(sfd, buf, strlen(buf), 0, (struct sockaddr*)&servaddr, sizeof(servaddr));
        sendto(sfd1, buf, strlen(buf), 0, (struct sockaddr*)&servaddr1, sizeof(servaddr1));
    }

    fclose(file);
    usleep(110000);
    exit(0);

}

void chatter_cb(const void *msg)
{
  uint32_t str_len = *((uint32_t *)msg);
  char buf[128] = {0};
  for (int i = 0; i < str_len && i < sizeof(buf)-1; i++)
    buf[i] = ((uint8_t *)msg)[4+i];
  printf("I heard: [%s]\n", buf);
  puts("::::::::::::::::::::::::::::::::::::::::::::::::::::\n");
}

int main(int argc, char **argv)
{
  printf("hello, world!\r\n");
  freertps_system_init();
  freertps_create_sub("chatter", 
                      "std_msgs::msg::dds_::String_",
                      chatter_cb);
  frudp_disco_start(); // we're alive now; announce ourselves to the world
  int cnt = 0;
 pthread_t tid1;
    pthread_create(&tid1,NULL,thread_worker1,argv[1]);
 while (freertps_system_ok())
  {
test_pthread(tid1);
    frudp_listen(1000000);
    frudp_disco_tick();
    printf("************************ %d ************************\n\n\n", ++cnt);
  }
  frudp_fini();
  return 0;
}

