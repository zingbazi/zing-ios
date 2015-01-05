// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

#import <UIKit/UIKit.h>

@interface AppController : NSObject<UIAccelerometerDelegate, UIApplicationDelegate>
{
}
- (void) startUnity:(UIApplication*)application;
- (void) Repaint;
- (void) RepaintDisplayLink;
- (void) prepareRunLoop;
@end

int		UnityGetAccelerometerFrequency();
