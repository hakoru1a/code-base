import type { PropsWithChildren } from 'react';
import type { ZoomProps } from '@mui/material/Zoom';
import type { SlideProps } from '@mui/material/Slide';

import React, { forwardRef } from 'react';

import Zoom from '@mui/material/Zoom';
import Slide from '@mui/material/Slide';

const RenderSlideTransition = (direction: SlideProps['direction']) =>
  forwardRef((props: SlideProps & PropsWithChildren, ref: React.Ref<unknown>) => <Slide direction={direction} ref={ref} {...props} />);

const RenderZoomTransition = () => forwardRef((props: ZoomProps, ref: React.Ref<unknown>) => <Zoom ref={ref} {...props} />);

const SlideTransition = {
  Up: RenderSlideTransition('up'),
  Down: RenderSlideTransition('down'),
  Left: RenderSlideTransition('left'),
  Right: RenderSlideTransition('right')
};

export const TransitionDialog = {
  Slide: SlideTransition,
  Zoom: RenderZoomTransition()
};
