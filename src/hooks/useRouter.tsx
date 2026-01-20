import { useNavigate } from 'react-router';
import { useMemo } from 'react';

const useRouter = () => {
  const navigate = useNavigate();

  return useMemo(
    () => ({
      back: () => navigate(-1),
      forward: () => navigate(1),
      refresh: () => navigate(0),
      push: (href: string, state?: DynamicObject) =>
        navigate(href, {
          state: {
            ...state
          }
        }),
      replace: (href: string, state?: DynamicObject) =>
        navigate(href, {
          state: {
            ...state
          },
          replace: true
        }),
      search: (href: string, search: string) =>
        navigate({
          pathname: href,
          search
        })
    }),
    [navigate]
  );
};

export default useRouter;
