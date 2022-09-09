//
//  TtiggerNetworkPermission.c
//  Unity-iPhone
//
//  Created by dudu on 2022/2/25.
//

#include "TriggerNetworkPermission.h"
#include <ifaddrs.h>
#include <sys/socket.h>
#include <net/if.h>
#include <netinet/in.h>
#import <Foundation/Foundation.h>

void triggerNetworkPermission(){
    
    NSURLSession* session = [NSURLSession sharedSession];
    
    NSURLSessionDataTask* task = [session dataTaskWithURL:[NSURL URLWithString:@"https://www.baidu.com"] completionHandler:^(NSData * _Nullable data, NSURLResponse * _Nullable response, NSError * _Nullable error) {
        NSLog(@"triggerNetworkPermission : %@",response);
    }];
    
    [task resume];
}
